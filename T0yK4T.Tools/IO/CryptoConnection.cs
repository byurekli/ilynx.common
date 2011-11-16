using System;
//using ProtoSharp.Core;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SkeinFish;
using System.Security;
using System.Security.Cryptography;
using T0yK4T.Tools.Cryptography;
using System.Threading;
using System.Reflection;

namespace T0yK4T.Tools.IO
{
    /// <summary>
    /// A delegate used in <see cref="CryptoConnection"/> to notify of a received packet
    /// </summary>
    /// <param name="packet">Will contain the read packet</param>
    /// <param name="wireLength">Will contain the total length of the received data (Note, due to encryption and encoding, this value may differ from the size of the packet "in memory")</param>
    public delegate void PacketReceivedDelegate(Packet packet, int wireLength);

    /// <summary>
    /// A delegate used in <see cref="CryptoConnection"/> to notify of a closed connection
    /// </summary>
    /// <param name="which">The cryptoconnection that was closed</param>
    /// <param name="reason">The "reason" for the disconnect</param>
    public delegate void DisconnectedDelegate(CryptoConnection which, DisconnectReason reason);

    /// <summary>
    /// An enum that specifies "why" a connection was closed
    /// </summary>
    public enum DisconnectReason
    {
        /// <summary>
        /// Specifies that the connection was closed gracefully
        /// </summary>
        Disconnect,

        /// <summary>
        /// Some error occured and the connection has been closed
        /// </summary>
        Error,
    }

    /// <summary>
    /// Class that can be used to read and write encrypted data to and from a network stream
    /// </summary>
    public class CryptoConnection : ComponentBase
    {
        //private PacketReceivedDelegate prd;
        //private DisconnectedDelegate dcd;

        /// <summary>
        /// This event is fired whenever the -remote- party disconnects, or an unrecoverable error occurs
        /// </summary>
        public event DisconnectedDelegate Disconnected;

        /// <summary>
        /// Called when <see cref="CryptoConnectionFlags.ManualRead"/> is UNset, and a packet is received
        /// </summary>
        public event PacketReceivedDelegate PacketReceived;

        private Stream netStream;
        private StreamReader reader;
        private StreamWriter writer;
        private Socket socket;
        private ILogger logger;

        private EncryptionProvider decryptor;
        private CryptoStream inputStream;
        private EncryptionProvider encryptor;
        private CryptoStream outputStream;
        
        private object p_Lock = new object();
        
        private Guid connectionID = Guid.NewGuid();
        private Guid remoteID = Guid.Empty;
        private RSAHelper privRSA;
        private HandshakeHelper handshaker;
        
#if DEBUG
        private int timeout = 1000;
#else
        private int timeout = 500;
#endif
        private int maxReadErrors = 5; // Note: This is sequential read errors
        private Thread backgroundWorker;
        
        private object readLock = new object();
        private object writeLock = new object();
        private Dictionary<uint, Action<Packet>> definedTypes = new Dictionary<uint, Action<Packet>>();
        private SynchronizationContext context;
        private int readErrors = 0;
        private CryptoConnectionFlags setupFlags = CryptoConnectionFlags.ManualRead;
        //private EndPoint RemoteEndPoint;
        private Queue<PacketWithSize> packetQueue = new Queue<PacketWithSize>();
        private const int CHUNK_SIZE = 512;
        private const int maxQueueSize = 20;
        
        private DateTime lastHandshake = DateTime.MinValue;

        private struct PacketWithSize
        {
            public Packet Packet;
            public int Size;
        }
        /// <summary>
        /// Gets the time of the last handshake made on this connection
        /// </summary>
        public DateTime LastHandshake
        {
            get { return this.lastHandshake; }
        }

#if DEBUG
        private TimeSpan maxKeyAge = TimeSpan.FromSeconds(5);
#else
        private TimeSpan maxKeyAge = TimeSpan.FromHours(1);
#endif

        /// <summary>
        /// Gets and Sets the Maximum (local) time a session key may be used
        /// </summary>
        public TimeSpan MaxKeyAge
        {
            get { return this.maxKeyAge; }
            set
            {
                lock (this.p_Lock)
                    this.maxKeyAge = value;
            }
        }

        private TimeSpan maxAgeSkew = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets and Sets the maximum amount of time "skew" for Session Key ages
        /// </summary>
        public TimeSpan MaxAgeSkew
        {
            get { return this.maxAgeSkew; }
            set
            {
                lock (this.p_Lock)
                    this.maxAgeSkew = value;
            }
        }

        private RunFlags runFlags = RunFlags.Run;


        /// <summary>
        /// Initializes a new instance of <see cref="CryptoConnection"/> and sets the logger to the specified value
        /// <para/>
        /// Please note that this constructor will also initialize a new <see cref="RSAHelper"/> which may take a while, depending on the setup
        /// </summary>
        /// <param name="logger"></param>
        public CryptoConnection(ILogger logger)
        {
            this.Init(logger);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CryptoConnection"/> and sets the logger and connectionflags to the specified values
        /// </summary>
        /// <param name="logger">The logger to use for logging</param>
        /// <param name="flags">The <see cref="CryptoConnectionFlags"/> butmask to set</param>
        public CryptoConnection(ILogger logger, CryptoConnectionFlags flags)
        {
            this.Init(logger);
            this.setupFlags = flags;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CryptoConnection"/> and sets the logger and flags to the specified values,
        /// <para/>
        /// This constructor will also call <see cref="CryptoConnection.WrapSocket(Socket)"/> with the specified <see cref="Socket"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="s"></param>
        /// <param name="flags"></param>
        public CryptoConnection(ILogger logger, Socket s, CryptoConnectionFlags flags)
        {
            this.Init(logger);
            this.setupFlags = flags;
            if (s == null)
                throw new ArgumentNullException("s");
            this.WrapSocket(s);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CryptoConnection"/> and sets the logger to the specified value,
        /// <para/>
        /// the internal networkstream used for sending and receiving packets will be retrieved from the specified <see cref="Socket"/>
        /// <para/>
        /// this will also call the <see cref="CryptoConnection.WrapSocket(Socket)"/> method with the specified <see cref="Socket"/>
        /// </summary>
        /// <param name="logger">The logger to use...</param>
        /// <param name="s">The socket to wrap</param>
        public CryptoConnection(ILogger logger, Socket s)
        {
            this.Init(logger);
            if (s == null)
                throw new ArgumentNullException("s");
            this.WrapSocket(s);
        }

        private void Init(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            this.logger = logger;
            this.privRSA = new RSAHelper(this.logger);
            this.handshaker = new HandshakeHelper(this);
            this.definedTypes.Add((uint)PacketType.HandshakeRequest, new Action<Packet>(this.HandleHandshakeRequest));
            this.definedTypes.Add((uint)PacketType.InitHandshake, new Action<Packet>(this.HandleInitHandshake));
            this.definedTypes.Add((uint)PacketType.InitPartialHandshake, new Action<Packet>(this.HandleInitPartialHandshake));
            this.definedTypes.Add((uint)PacketType.DisconnectNotification, new Action<Packet>(this.HandleDisconnect));
            this.definedTypes.Add((uint)PacketType.ConnectionIDExchange, new Action<Packet>(this.HandleConnectionID));
        }

        /// <summary>
        /// Begins accepting a handshake sent by the other side of this connection
        /// </summary>
        public void WrapSocket(Socket s)
        {
            lock (this.p_Lock)
            {
                this.context = SynchronizationContext.Current;
                if (this.context == null)
                    this.context = new SynchronizationContext();

                this.socket = s;
                bool succesful = this.handshaker.Handshake(s, this.privRSA, out this.encryptor, out this.decryptor);
                if (!succesful)
                {
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Close();
                    this.socket = null;
                    throw new Exception("Unable to complete handshake, see log for details");
                }
                else
                {
                    this.encryptor.Reset();
                    this.decryptor.Reset();

                    this.AddRunFlag(RunFlags.IsConnected);
                    this.lastHandshake = DateTime.Now;

                    this.netStream = new NetworkStream(this.socket);
                    this.inputStream = new CryptoStream(this.netStream, this.decryptor.Decryptor, CryptoStreamMode.Read);
                    this.outputStream = new CryptoStream(this.netStream, this.encryptor.Encryptor, CryptoStreamMode.Write);

                    this.netStream.ReadTimeout = this.timeout;
                    this.netStream.WriteTimeout = this.timeout;

                    this.reader = new StreamReader(this.netStream);
                    this.writer = new StreamWriter(this.netStream);

                    this.backgroundWorker = new Thread(new ThreadStart(this.ReadPackets));
                    this.backgroundWorker.Start();
                    
                    this.SendConnectionID();
                }
            }
        }

        private void ReadPackets()
        {
            try
            {
                bool cont = true;
                
                while (cont && this.CheckRunFlags(RunFlags.Run))
                {
                    try
                    {
                        int size;
                        Packet packet = this.Read(out size);
                        cont = this.HandlePacket(packet, size);
                    }
                    catch (IOException e) { this.HandleException(e); }
                    try { cont &= this.CheckSessionKeyExpiry(); }
                    catch { }

                    cont &= this.CheckRunFlags(RunFlags.IsConnected);
                    cont &= this.readErrors < this.maxReadErrors;
                }

                if (readErrors >= this.maxReadErrors)
                {
                    base.LogWarning("Connection with {0} has too many read errors, dropping", this.socket.RemoteEndPoint);
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Close();
                }
                this.RemoveRunFlag(RunFlags.IsConnected);
                base.LogInformation("Connection to {0} Closed", this.socket.RemoteEndPoint);
            }
            catch (ThreadAbortException)
            {
                if (!this.CheckRunFlags(RunFlags.DontThrowOnAborted))
                    throw;
                else
                    return;
            }
        }

        private void HandleException(IOException e)
        {
            SocketException se;
            if ((se = e.InnerException as SocketException) != null)
            {
                switch (se.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                        this.RemoveRunFlag(RunFlags.IsConnected);
                        base.LogError("Connection to {0} Reset", this.socket.RemoteEndPoint);
                        this.OnDisconnect(DisconnectReason.Error);
                        break;
                    case SocketError.TimedOut:
                        break;
                    case SocketError.Interrupted:
                        if (!this.CheckRunFlags(RunFlags.Run))
                            break;
                        this.RemoveRunFlag(RunFlags.IsConnected);
                        this.OnDisconnect(DisconnectReason.Error);
                        base.LogError("Connection to {0} Reset", this.socket.RemoteEndPoint);
                        break;
                    case SocketError.Shutdown:
                        this.RemoveRunFlag(RunFlags.IsConnected);
                        // This <should> be normal...
                        break;
                    case SocketError.ConnectionAborted:
                        if (!this.CheckRunFlags(RunFlags.Run))
                            break;
                        this.RemoveRunFlag(RunFlags.IsConnected);
                        this.OnDisconnect(DisconnectReason.Error);
                        base.LogError("Connection to {0} Reset", this.socket.RemoteEndPoint);
                        break;
                    default:
                        base.LogException(se, System.Reflection.MethodBase.GetCurrentMethod());
                        break;
                }
            }
            else if (e.InnerException is ObjectDisposedException)
            {
                base.LogInformation("Connection to {0} Disappeared", this.socket.RemoteEndPoint);
                this.RemoveRunFlag(RunFlags.Run);
                this.RemoveRunFlag(RunFlags.IsConnected);
                this.OnDisconnect(DisconnectReason.Error);
            }
            else
            {
                base.LogException(e, System.Reflection.MethodBase.GetCurrentMethod());
                base.LogWarning("Closing connection to {0}, got an unhandled exception on this end", this.socket.RemoteEndPoint);
                this.OnDisconnect(DisconnectReason.Error);
            }
        }

        private bool HandlePacket(Packet packet, int size)
        {
            if (packet == null)
            {
                bool dataAvailableOrConnectionReset = this.socket.Poll(1000, SelectMode.SelectRead);
                bool dataAvailable = this.socket.Available != 0;
                if (dataAvailable && dataAvailableOrConnectionReset)
                    readErrors++;
                else // Disconnected
                    return false;
            }
            else
            {
                readErrors = 0;
                Action<Packet> predefinedAction;
                if (this.definedTypes.TryGetValue((uint)packet.TypeID, out predefinedAction))
                {
                    predefinedAction(packet);
                    if ((this.setupFlags & CryptoConnectionFlags.PassOn) == CryptoConnectionFlags.PassOn)
                        this.OnPacketReceived(packet, size);
                }
                else
                    this.OnPacketReceived(packet, size);
            }
            return true;
        }

        private bool CheckSessionKeyExpiry()
        {
            if ((DateTime.Now - this.lastHandshake) >= this.maxKeyAge)
            {
                if (this.CheckRunFlags(RunFlags.LocalHandshakeRequested) && (DateTime.Now - lastHandshake) >= this.maxKeyAge + this.maxAgeSkew)
                {
                    base.LogCritical("Remote host did not want to renegotiate session keys, closing connection");
                    this.Close();
                    return false;
                }
                else if (!this.CheckRunFlags(RunFlags.LocalHandshakeRequested))
                {
                    this.AddRunFlag(RunFlags.LocalHandshakeRequested);
                    this.AddRunFlag(RunFlags.IsBlocking);
                    Packet hsRequest = new Packet { TypeID = (int)PacketType.HandshakeRequest };
                    this.WritePacketInternal(hsRequest);
                }
            }
            return true;
        }

        private void HandleDisconnect(Packet packet)
        {
            if (!this.CheckRunFlags(RunFlags.DisconnectReceived))
            {
                this.AddRunFlag(RunFlags.DisconnectReceived);
                int errors = 0;
                while (this.socket.Available > 0)
                {
                    try
                    {
                        int size = 0;
                        Packet pkt = this.Read(out size);
                        this.HandlePacket(pkt, size);
                    }
                    catch
                    {
                        errors++;
                        if (errors >= 5)
                            break;
                    }
                }

                this.RemoveRunFlag(RunFlags.Run);
                this.RemoveRunFlag(RunFlags.IsConnected);
                this.OnDisconnect(DisconnectReason.Disconnect);
                this.Close();
            }
        }

        private void HandleConnectionID(Packet packet)
        {
            Guid remoteID = new Guid(packet.Data);
            if (remoteID == this.connectionID)
            {
                this.connectionID = Guid.NewGuid();
                this.SendConnectionID();
            }
        }

        private void SendConnectionID()
        {
            Packet packet = new Packet { TypeID = (int)PacketType.ConnectionIDExchange, Data = this.connectionID.ToByteArray() };
            this.WritePacketInternal(packet);
        }

        private void PartialHandshake()
        {
            this.AddRunFlag(RunFlags.IsBlocking);
            lock (this.p_Lock)
            {
                Packet reply = new Packet { TypeID = (int)PacketType.InitPartialHandshake };
                this.WritePacketInternal(reply);
                int size;
                Packet received = this.Read(out size);
                if (received.TypeID != (int)PacketType.InitPartialHandshake) // This should never happen
                    base.LogError("Remote host did not respond to InitPartialHandshake in a manner that could be understood...");
                else
                {
                    base.LogInformation("Starting partial key exchange with remote host");
                    RSAHelper remotePubRSA;
                    HandshakeHelper.ExchangePubKey(this.netStream, this.privRSA, out remotePubRSA);
                    string read = this.reader.ReadLine();
                    byte[] rsaDecryptedResponse = this.privRSA.DecryptBase64String(read);
                    Packet remoteKey = ToySerializer.Deserialize<Packet>(rsaDecryptedResponse);
                    this.decryptor = HandshakeHelper.GetDecryptor(this.privRSA, remoteKey);

                    reply.TypeID = (int)PacketType.EndPartialHandshake;
                    this.WritePacketInternal(reply);

                    //Recreate input stream
                    this.inputStream = new CryptoStream(this.netStream, this.decryptor.Decryptor, CryptoStreamMode.Read);

                    received = this.Read(out size);
                    if (received == null)
                    {
                        base.LogCritical("Partial SessionKey renegotiation has failed for remote endpoint {0}, connection closed", this.socket.RemoteEndPoint);
                        this.Close();
                    }
                    else
                        base.LogInformation("Partial SessionKey renegotiation for remote endpoint {0} has succeeded", this.socket.RemoteEndPoint);
                }
            }
            this.RemoveRunFlag(RunFlags.IsBlocking);
        }

        private void HandleInitHandshake(Packet packet)
        {
            if (this.CheckRunFlags(RunFlags.LocalHandshakeRequested))
            {
                this.RemoveRunFlag(RunFlags.LocalHandshakeRequested);
                this.AddRunFlag(RunFlags.IsBlocking);
                lock (this.p_Lock)
                {
                    this.WritePacketInternal(packet);
                    this.FullHandshake();
                }
                this.RemoveRunFlag(RunFlags.IsBlocking);
            }
            else // This should never happen
                base.LogError("FIXME! Got InitHandshake from remote without any indications of a handshake having been requested!");
        }

        private void HandleHandshakeRequest(Packet packet)
        {
            base.LogDebug("Got HandshakeRequest from {0}", this.socket.RemoteEndPoint);
            if ((DateTime.Now - this.lastHandshake) < maxKeyAge - maxAgeSkew && !this.CheckRunFlags(RunFlags.LocalHandshakeRequested))
                this.PartialHandshake();
            else
            {
                if (this.CheckRunFlags(RunFlags.LocalHandshakeRequested))
                {
                    byte[] localID = this.connectionID.ToByteArray();
                    byte[] remoteConID = this.remoteID.ToByteArray();
                    for (int i = 0; i < localID.Length; i++)
                    {
                        if (localID[i] < remoteConID[i]) // Guid breaks race
                            return;
                    }
                }

                this.AddRunFlag(RunFlags.IsBlocking);
                lock (this.p_Lock)
                {
                    Packet reply = new Packet { TypeID = (int)PacketType.InitHandshake };
                    this.WritePacketInternal(reply);
                    int size;
                    Packet response = this.Read(out size);
                    if (response.TypeID == (uint)PacketType.InitHandshake)
                        this.FullHandshake();
                    else // This should never happen
                        base.LogError("Got last-second CancelHandshake from remote, is this intended behaviour?");
                }
                this.RemoveRunFlag(RunFlags.LocalHandshakeRequested);
                this.RemoveRunFlag(RunFlags.IsBlocking);
            }
        }

        private void FullHandshake()
        {
            if (this.handshaker.Handshake(this.netStream, this.socket, this.privRSA, out this.encryptor, out this.decryptor, ref this.readLock, ref this.writeLock))
            {
                this.outputStream = new CryptoStream(this.netStream, this.encryptor.Encryptor, CryptoStreamMode.Write);
                this.inputStream = new CryptoStream(this.netStream, this.decryptor.Decryptor, CryptoStreamMode.Read);
                this.lastHandshake = DateTime.Now;
            }
            else
            {
                base.LogCritical("Unable to complete handshake - unrecoverable");
                this.Close();
            }

        }

        private void HandleInitPartialHandshake(Packet packet)
        {
            if (this.CheckRunFlags(RunFlags.LocalHandshakeRequested))
            {
                this.AddRunFlag(RunFlags.IsBlocking);
                lock (this.p_Lock)
                {
                    this.WritePacketInternal(packet);
                    RSAHelper remotePubKey;
                    HandshakeHelper.ExchangePubKey(this.netStream, this.privRSA, out remotePubKey);
                    this.encryptor = new EncryptionProvider();
                    Packet sentPacket = HandshakeHelper.WriteEncryptor(remotePubKey, this.encryptor);
                    byte[] serializedEncryptorPacket = ToySerializer.Serialize(sentPacket);
                    this.writer.WriteLine(remotePubKey.EncryptToBase64String(serializedEncryptorPacket));
                    this.writer.Flush();
                    
                    //Recreate output stream
                    this.outputStream = new CryptoStream(this.netStream, this.encryptor.Encryptor, CryptoStreamMode.Write);
                    
                    int size;
                    Packet remoteResponse = this.Read(out size);
                    if (remoteResponse == null)
                    {
                        base.LogCritical("Partial SessionKey renegotiation has failed for remote endpoint {0}, connection closed", this.socket.RemoteEndPoint);
                        this.Close();
                    }
                    else
                    {
                        this.WritePacketInternal(remoteResponse);
                        base.LogInformation("Partial SessionID renegotiation succeded for remote host {0}", this.socket.RemoteEndPoint);
                        this.lastHandshake = DateTime.Now;
                    }
                }
                this.RemoveRunFlag(RunFlags.LocalHandshakeRequested);
                this.RemoveRunFlag(RunFlags.IsBlocking);
            }
        }

        private void HandleCancelHandshake(Packet packet)
        {
            if (this.CheckRunFlags(RunFlags.LocalHandshakeRequested))
            {
                base.LogWarning("Got CancelHandshake from remote endpoint {0}, Local has requested handshake", this.socket.RemoteEndPoint);
                base.LogWarning("Session keys are {0} minutes old, local maximum age is {1} minues (+- {2} minues)", (DateTime.Now - this.lastHandshake).TotalMinutes.ToString("f2"), maxKeyAge.TotalMinutes.ToString("f2"), maxAgeSkew.TotalMinutes.ToString("f2"));
            }
            else
                base.LogWarning("Got CancelHandshake from remote endpoint {0}, Local has NOT requested handshake, this should never happen - Is this intended behaviour?", this.socket.RemoteEndPoint);
            this.RemoveRunFlag(RunFlags.IsBlocking);
        }
        
        private void NotifyDisconnect()
        {
            Packet packet = new Packet { TypeID = (int)PacketType.DisconnectNotification };
            this.WritePacketInternal(packet);
        }

        private Queue<PacketReceivedArgs> queuedEvents = new Queue<PacketReceivedArgs>();

        private void OnPacketReceived(Packet packet, int size)
        {
            if ((this.ConnectionFlags & CryptoConnectionFlags.ManualRead) == CryptoConnectionFlags.ManualRead)
            {
                PacketWithSize pws = new PacketWithSize { Packet = packet, Size = size };
                while (packetQueue.Count >= maxQueueSize) // Choking
                    Thread.Sleep(10);
                this.packetQueue.Enqueue(pws);
            }
            else
            {
                PacketReceivedArgs args = new PacketReceivedArgs { P = packet, S = size, D = this.PacketReceived };
                if (this.PacketReceived != null)
                {
                    this.context.Post(new SendOrPostCallback((o) =>
                    {
                        PacketReceivedArgs a = (PacketReceivedArgs)o;
                        a.D.Invoke(a.P, a.S);
                    }), args);
                }
                else
                {
                    while (this.queuedEvents.Count >= maxQueueSize) // Choking
                        Thread.Sleep(10);
                    this.queuedEvents.Enqueue(args);
                }
            }
        }

        ///<summary>
        /// Manually reads a packet from the underlying networkstream
        /// <para/>
        /// Please note that this will only return anything if <see cref="CryptoConnection.ConnectionFlags"/> has the <see cref="CryptoConnectionFlags.ManualRead"/> bitfield set
        /// </summary>
        /// <param name="timeout">The timeout (in milliseconds) to wait before throwing a TimeoutException</param>
        /// <param name="size">The final size of the packet</param>
        /// <returns>The packet that was read</returns>
        public Packet ReadPacket(int timeout, out int size)
        {
            DateTime opStart = DateTime.Now;
            while (this.packetQueue.Count < 1)
            {
                Thread.Sleep(1);
                if ((DateTime.Now - opStart).TotalMilliseconds >= timeout)
                    throw new TimeoutException("Operation timed out");
                else if (!this.Connected)
                    throw new InvalidOperationException("Disconnected");
            }
            PacketWithSize pws = this.packetQueue.Dequeue();
            size = pws.Size;
            return pws.Packet;
        }
        
        ///<summary>
        /// Manually reads a packet from the underlying networkstream
        /// <para/>
        /// Please note that this will only return anything if <see cref="CryptoConnection.ConnectionFlags"/> has the <see cref="CryptoConnectionFlags.ManualRead"/> bitfield set
        /// </summary>
        /// <param name="size">The final size of the packet that was read</param>
        /// <returns>The packet that was read</returns>
        public Packet ReadPacket(out int size)
        {
            while (this.packetQueue.Count < 1)
            {
                Thread.Sleep(1);
                if (!this.Connected)
                    throw new InvalidOperationException("Disconnected");
            }
            PacketWithSize pws = this.packetQueue.Dequeue();
            size = pws.Size;
            return pws.Packet;
        }

        private void OnDisconnect(DisconnectReason reason)
        {
            if (this.Disconnected != null)
            {
                this.context.Post(new SendOrPostCallback((o) =>
                    {
                        this.Disconnected.Invoke(this, (DisconnectReason)o);
                    }), reason);
            }
        }

        /// <summary>
        /// Gets a value indicating wether this connection is still considered as being connected
        /// </summary>
        public bool Connected
        {
            get { return this.CheckRunFlags(RunFlags.IsConnected); }
        }

        /// <summary>
        /// Sends the specified <see cref="Packet"/> to the remote host
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public int SendPacket(Packet packet)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");
            else if (this.CheckRunFlags(RunFlags.IsConnected))
            {
                int ret;
                ret = this.WritePacket(packet);
                return ret;
            }
            else
                throw new Exception("Not connected...");
        }

        /// <summary>
        /// Closes the underlying socket and disposes it
        /// </summary>
        public void Close()
        {
            lock (this.p_Lock)
            {
                if (this.CheckRunFlags(RunFlags.IsConnected) && this.socket.Connected)
                    this.NotifyDisconnect();

                this.RemoveRunFlag(RunFlags.All);
                if (this.socket != null)
                {
                    if (this.socket.Connected)
                        this.socket.Shutdown(SocketShutdown.Receive);
                }
            }
        }

        [Flags]
        private enum RunFlags : byte
        {
            Run = 0x01,
            LocalHandshakeRequested = 0x02,
            NegotiateMaxKeyAge = 0x04,
            IsConnected = 0x08,
            IsBlocking = 0x10,
            DontThrowOnAborted = 0x20,
            DisconnectReceived = 0x30,
            All = 0xff,
        }

        private struct PacketReceivedArgs
        {
            public Packet P;
            public int S;
            public PacketReceivedDelegate D;
        }

        /// <summary>
        /// Attempts to connect to the specified <see cref="IPEndPoint"/>
        /// <para/>
        /// Note, this method is more of a convenience than anything useful, it simply connects a <see cref="TcpClient"/> to the specified
        /// <para/>
        /// endpoint and calls the <see cref="CryptoConnection.WrapSocket(Socket)"/> method of this instance
        /// <para/>
        /// returns true if connected, otherwise false
        /// </summary>
        /// <param name="ep">The remote <see cref="IPEndPoint"/> to attempt to connect to</param>
        /// <returns>True if succesful, otherwise false</returns>
        public bool ConnectTo(IPEndPoint ep)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ep);
                if (client.Connected)
                    this.WrapSocket(client.Client);
            }
            catch (Exception e) { base.LogException(e, System.Reflection.MethodBase.GetCurrentMethod()); }
            return this.CheckRunFlags(RunFlags.IsConnected);
        }

        /// <summary>
        /// Attempts to connect to the specified host on the specified port
        /// </summary>
        /// <param name="hostname">The hostname of the host to connect to</param>
        /// <param name="port">The port to connect to</param>
        /// <returns>True if succesful, otherwise false</returns>
        public bool ConnectTo(string hostname, ushort port)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(hostname, port);
                if (client.Connected)
                    this.WrapSocket(client.Client);
            }
            catch (Exception e) { base.LogException(e, MethodBase.GetCurrentMethod()); }
            return this.CheckRunFlags(RunFlags.IsConnected);
        }

        /// <summary>
        /// Gets and Sets the logger used when logging data
        /// </summary>
        protected override ILogger Logger
        {
            get { return this.logger; }
            set { this.logger = value; }
        }

        /// <summary>
        /// Gets or Sets the <see cref="CryptoConnectionFlags"/> to use
        /// <para/>
        /// Please note that this defaults to <see cref="CryptoConnectionFlags.ManualRead"/>
        /// </summary>
        public CryptoConnectionFlags ConnectionFlags
        {
            get { return this.setupFlags; }
            set
            {
                lock (this.p_Lock)
                {
                    CryptoConnectionFlags previousFlags = this.setupFlags;
                    this.setupFlags = value;
                    if (this.queuedEvents.Count > 0 &&
                        (value & CryptoConnectionFlags.ManualRead) == CryptoConnectionFlags.ManualRead &&
                        (previousFlags & CryptoConnectionFlags.ManualRead) != CryptoConnectionFlags.ManualRead)
                    {
                        while (this.queuedEvents.Count > 0)
                        {
                            PacketReceivedArgs args = this.queuedEvents.Dequeue();
                            this.packetQueue.Enqueue(new PacketWithSize { Packet = args.P, Size = args.S });
                        }
                    }
                    else if ((value & CryptoConnectionFlags.ManualRead) != CryptoConnectionFlags.ManualRead && (previousFlags & CryptoConnectionFlags.ManualRead) == CryptoConnectionFlags.ManualRead)
                    {
                        while (this.packetQueue.Count > 0)
                        {
                            PacketWithSize pws = this.packetQueue.Dequeue();
                            this.OnPacketReceived(pws.Packet, pws.Size);
                        }
                    }
                }
            }
        }

        private int WritePacket(Packet packet)
        {
            int finalSize;
            while (this.CheckRunFlags(RunFlags.IsBlocking))
            {
                Thread.Sleep(1);
                if (!this.Connected)
                    throw new InvalidOperationException("The connection has been closed");
            }
            lock (this.writeLock)
                WriteBlocks(packet.GetBytes(), this.outputStream, this.encryptor.BlockSize, out finalSize);
            return finalSize;
        }

        private int WritePacketInternal(Packet packet)
        {
            int finalSize;
            lock (this.writeLock)
                WriteBlocks(packet.GetBytes(), this.outputStream, this.encryptor.BlockSize, out finalSize);
            return finalSize;
        }

        private Packet Read(out int finalSize)
        {
            byte[] received;
            lock (this.readLock)
                received = ReadBlocks(this.inputStream, this.decryptor.BlockSize, out finalSize);
            return Packet.FromBytes(received);
        }

        private static byte[] ReadBlocks(CryptoStream inputStream, int blockSize, out int finalSize)
        {
            // Read length field
            byte[] lengthField = new byte[sizeof(int)];
            finalSize = inputStream.Read(lengthField, 0, lengthField.Length);
            int length = BitConverter.ToInt32(lengthField, 0);
            
            //Read data field
            byte[] actualData = new byte[length];
            int read = inputStream.Read(actualData, 0, actualData.Length);
            finalSize += read;
            if (read != length)
                throw new CryptographicUnexpectedOperationException("Unexpected data length");

            //Read padding field
            int rem = (blockSize - (read % blockSize)) - sizeof(int);
            byte[] rndData = new byte[rem];
            read = inputStream.Read(rndData, 0, rndData.Length);
            finalSize += read;
            if (read != rem)
                throw new CryptographicUnexpectedOperationException("Unexpected padding length");

            return actualData;
        }

        private static void WriteBlocks(byte[] data, CryptoStream outputStream, int blockSize, out int finalSize)
        {
            int rem = (blockSize - (data.Length % blockSize)) - sizeof(int);
            byte[] rnd = new byte[rem]; // Padding
            CryptoCommon.Prng.GetBytes(rnd);
            byte[] lengthField = BitConverter.GetBytes(data.Length);
            outputStream.Write(lengthField, 0, lengthField.Length);
            outputStream.Write(data, 0, data.Length);
            outputStream.Write(rnd, 0, rnd.Length);

            finalSize = lengthField.Length + data.Length + rnd.Length;
        }

        private bool CheckRunFlags(RunFlags flag)
        {
            return (this.runFlags & flag) == flag;
        }

        private void AddRunFlag(RunFlags flag)
        {
            this.runFlags |= flag;
        }

        private void RemoveRunFlag(RunFlags flag)
        {
            this.runFlags = (this.runFlags | flag) ^ flag;
        }

        /// <summary>
        /// Adds the specified flag to the <see cref="CryptoConnectionFlags"/> (<see cref="CryptoConnection.ConnectionFlags"/>) of this instance
        /// </summary>
        /// <param name="flag"></param>
        public void SetFlag(CryptoConnectionFlags flag)
        {
            this.ConnectionFlags |= flag;
        }

        /// <summary>
        /// Removes the specified flag from the <see cref="CryptoConnectionFlags"/> (<see cref="CryptoConnection.ConnectionFlags"/>) of this instance
        /// </summary>
        /// <param name="flag"></param>
        public void UnSetFlag(CryptoConnectionFlags flag)
        {
            this.ConnectionFlags = (this.ConnectionFlags | flag) ^ flag;
        }

        /// <summary>
        /// Gets the ID of this connection (Note: This may change during connection setup exchanges!)
        /// </summary>
        public Guid ConnectionID
        {
            get { return this.connectionID; }
        }

        /// <summary>
        /// Gets the remote endpoint of the socket this connection is currently wrapping
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get { return this.socket.RemoteEndPoint; }
        }

        /// <summary>
        /// Gets the local endpoint of the socket this connection is currently wrapping
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get { return this.socket.LocalEndPoint; }
        }
    }

    /// <summary>
    /// This enum contains flags that can be passed to a <see cref="CryptoConnection"/> to change it's behaviour in some way
    /// </summary>
    [Flags]
    public enum CryptoConnectionFlags
    {
        /// <summary>
        /// None...
        /// </summary>
        None = 0x00,

        /// <summary>
        /// If this bit is set, the connection will pass on internally handled packets
        /// <para/>
        /// See <see cref="Packet"/>
        /// </summary>
        PassOn = 0x01,

        /// <summary>
        /// If this bit is set, the connection will not call the packetreceived callback
        /// <para/>
        /// You have to manually "read" the packets via ReadPacketM
        /// </summary>
        ManualRead = 0x02,
    }
}
