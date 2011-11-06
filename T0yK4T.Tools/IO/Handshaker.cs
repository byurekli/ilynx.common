using System;
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
//using ProtoSharp.Core;

namespace T0yK4T.Tools.IO
{
    /// <summary>
    /// A class that can be used to securely negotiate session encryption and decryption keys over a connected <see cref="Socket"/>
    /// </summary>
    public class HandshakeHelper
    {
        private static readonly Encoding handshakeEncoding = ASCIIEncoding.ASCII;
        private ComponentBase ownerComponent;

        /// <summary>
        /// Initializes a new instance of <see cref="HandshakeHelper"/>
        /// </summary>
        /// <param name="loggerComponent">This will be used to log information errors to</param>
        public HandshakeHelper(ComponentBase loggerComponent)
        {
            this.ownerComponent = loggerComponent;
        }

        /// <summary>
        /// Begins the handshake process on the specified <see cref="Socket"/>
        /// </summary>
        /// <param name="decryptor">This will be set to a <see cref="EncryptionProvider"/> that has been initialized with the negotiated (local) decryption key</param>
        /// <param name="encryptor">This will be set to a <see cref="EncryptionProvider"/> that has been initialized with the negotiated (remote) encryption key</param>
        /// <param name="privRSA">The <see cref="RSAHelper"/> to use to securely negotiate encryption / decryption keys</param>
        /// <param name="s">The socket to handshake on</param>
        /// <returns></returns>
        public bool Handshake(Socket s, RSAHelper privRSA, out EncryptionProvider encryptor, out EncryptionProvider decryptor)
        {
            RSAHelper pubRSA;
            NetworkStream netStream = new NetworkStream(s);
            encryptor = null; decryptor = null;
            try
            {
                this.ownerComponent.LogInformation("[{0}] -> [{1}]: Exchanging RSA Public Keys", s.LocalEndPoint, s.RemoteEndPoint);
                ExchangePubKey(netStream, privRSA, out pubRSA);

                this.ownerComponent.LogInformation("[{0}] -> [{1}]: Negotiating Session Keys", s.LocalEndPoint, s.RemoteEndPoint);
                NegotiateSessionKeys(netStream, out decryptor, out encryptor, pubRSA, privRSA);

                this.ownerComponent.LogInformation("[{0}] -> [{1}]: Verifying Connection...", s.LocalEndPoint, s.RemoteEndPoint);
                bool succes = VerifySessionKeys(netStream, encryptor, decryptor);
                this.ownerComponent.LogInformation("[{0}] -> [{1}]: {2}", s.LocalEndPoint, s.RemoteEndPoint, succes ? "Connected" : "Error");
                return succes;
            }
            catch (Exception e)
            {
                this.ownerComponent.LogException(e, System.Reflection.MethodBase.GetCurrentMethod());
                return false;
            }
            finally { netStream.Close(); }
        }

        /// <summary>
        /// Begins the handshake process on the specified <see cref="Socket"/>
        /// </summary>
        /// <param name="decryptor">This will be set to a <see cref="EncryptionProvider"/> that has been initialized with the negotiated (local) decryption key</param>
        /// <param name="encryptor">This will be set to a <see cref="EncryptionProvider"/> that has been initialized with the negotiated (remote) encryption key</param>
        /// <param name="privRSA">The <see cref="RSAHelper"/> to use to securely negotiate encryption / decryption keys</param>
        /// <param name="netStream">The already opened <see cref="NetworkStream"/> to use while shaking hands</param>
        /// <param name="s">The socket to handshake on</param>
        /// <param name="readLock">The object used to lock read access to the stream to one thread</param>
        /// <param name="writeLock">The object used to lock write access to the stream to one thread</param>
        /// <returns></returns>
        public bool Handshake(Stream netStream, Socket s, RSAHelper privRSA, out EncryptionProvider encryptor, out EncryptionProvider decryptor, ref object readLock, ref object writeLock)
        {
            RSAHelper pubRSA;
            encryptor = null; decryptor = null;
            try
            {
                lock (readLock)
                {
                    lock (writeLock)
                    {
                        this.ownerComponent.LogInformation("[{0}] -> [{1}]: Exchanging RSA Public Keys", s.LocalEndPoint, s.RemoteEndPoint);
                        ExchangePubKey(netStream, privRSA, out pubRSA);

                        this.ownerComponent.LogInformation("[{0}] -> [{1}]: Negotiating Session Keys", s.LocalEndPoint, s.RemoteEndPoint);
                        NegotiateSessionKeys(netStream, out decryptor, out encryptor, pubRSA, privRSA);

                        this.ownerComponent.LogInformation("[{0}] -> [{1}]: Verifying Connection...", s.LocalEndPoint, s.RemoteEndPoint);
                        bool succes = VerifySessionKeys(netStream, encryptor, decryptor);
                        this.ownerComponent.LogInformation("[{0}] -> [{1}]: {2}", s.LocalEndPoint, s.RemoteEndPoint, succes ? "Success" : "Error");
                        return succes;
                    }
                }
            }
            catch (Exception e)
            {
                this.ownerComponent.LogException(e, System.Reflection.MethodBase.GetCurrentMethod());
                return false;
            }
        }

        private bool VerifySessionKeys(Stream netStream, EncryptionProvider encryptor, EncryptionProvider decryptor)
        {
            StreamReader reader = new StreamReader(netStream);
            StreamWriter writer = new StreamWriter(netStream);

            Packet rec = new Packet();
            rec.Data = new byte[1];
            rec.TypeID = (int)PacketType.Handshake;
            rec.ChannelID = CryptoCommon.GetPrngInt(); // Adding an element of randomness to our sent data
            rec.UserID = CryptoCommon.GetPrngInt(); // Adding an element of randomness to our sent data

            WriteLine(writer, rec, encryptor);

            int size;
            rec = ReadLine(reader, decryptor, out size);

            if (rec == null)
                throw new Exception(string.Format("Unable to complete handshake, expected to read an encrypted packet from the input stream, got " + rec ?? "[nothing]"));
            if (rec.TypeID != (int)PacketType.Handshake)
                throw new Exception(string.Format("Unable to complete handshake, Expected packet with TypeID {0}", PacketType.Handshake));
            else
                return true;
        }

        private bool NegotiateSessionKeys(Stream netStream, out EncryptionProvider decryptor, out EncryptionProvider encryptor, RSAHelper pubRSA, RSAHelper privRSA)
        {
            StreamReader reader = new StreamReader(netStream);
            StreamWriter writer = new StreamWriter(netStream);
            encryptor = new EncryptionProvider();
            try
            {
                Packet packet = WriteEncryptor(pubRSA, encryptor);
                byte[] serializedPacket = ToySerializer.Serialize(packet);
                string sendData = pubRSA.EncryptToBase64String(serializedPacket);
                writer.WriteLine(sendData); // Writing the packet as a Base64 encoded string to the network stream in the current instance
                writer.Flush();

                string read = reader.ReadLine(); // Getting response
                packet = ToySerializer.Deserialize<Packet>(privRSA.DecryptBase64String(read));
                decryptor = GetDecryptor(privRSA, packet);
            }
            catch { decryptor = null; return false; }
            return true;
        }

        /// <summary>
        /// Exchanged public keys with the remote host on the other end of the specified <see cref="NetworkStream"/> (<paramref name="netStream"/>)
        /// <para/>
        /// an RSAHelper that can be used to RSA encrypt data going towards the remote host is created and returned via that <paramref name="pubRSA"/> parameter
        /// </summary>
        /// <param name="netStream">The <see cref="NetworkStream"/> to use for exchanging public keys</param>
        /// <param name="privRSA">The RSAHelper for the local party</param>
        /// <param name="pubRSA">Will be set to a new instance of <see cref="RSAHelper"/> that can be used to encrypt data going towards the remote party</param>
        public static void ExchangePubKey(Stream netStream, RSAHelper privRSA, out RSAHelper pubRSA)
        {
            StreamReader src = new StreamReader(netStream);
            StreamWriter dst = new StreamWriter(netStream);

            SendPubkey(dst, privRSA);
            pubRSA = ReceivePubkey(src);
        }

        private static int ActualWriteLine(StreamWriter dst, Packet packet)
        {
            byte[] serializedPacket = ToySerializer.Serialize(packet);
            string sendData = Convert.ToBase64String(serializedPacket);
            dst.WriteLine(sendData);
            dst.Flush();
            return sendData.Length;
        }

        private int WriteLine(StreamWriter dst, Packet packet, EncryptionProvider encryptor)
        {
            byte[] serializedPacket = ToySerializer.Serialize(packet);
            serializedPacket = encryptor.EncryptArray(serializedPacket);
            string sendData = Convert.ToBase64String(serializedPacket);
            dst.WriteLine(sendData);
            dst.Flush();
            return sendData.Length;
        }

        private Packet ActualReadLine(StreamReader src, out int finalSize)
        {
            string read = src.ReadLine();
            if (read == null)
                throw new NullReferenceException("Read null from StreamReader");
            finalSize = read.Length;
            Packet packet = ToySerializer.Deserialize<Packet>(Convert.FromBase64String(read));
            return packet;
        }

        private Packet ReadLine(StreamReader src, EncryptionProvider decryptor, out int finalSize)
        {
            string read = src.ReadLine();
            if (read == null)
                throw new NullReferenceException("Read null from StreamReader");
            finalSize = read.Length;
            byte[] data = Convert.FromBase64String(read);
            data = decryptor.DecryptArray(data);
            return ToySerializer.Deserialize<Packet>(data);
        }

        /// <summary>
        /// Gets an <see cref="EncryptionProvider"/> constructed with the Key and IV contained in the specified packet
        /// <para/>
        /// This method is the counterpart to <see cref="HandshakeHelper.WriteEncryptor(RSAHelper, EncryptionProvider)"/>
        /// </summary>
        /// <param name="rsa">The "local" <see cref="RSAHelper"/> to use for decrypting the Key and IV</param>
        /// <param name="pkt">The received packet</param>
        /// <returns></returns>
        public static EncryptionProvider GetDecryptor(RSAHelper rsa, Packet pkt)
        {
            MemoryStream dataStream = new MemoryStream(pkt.Data); // Creating a stream around the packet data for reading
            StreamReader dataReader = new StreamReader(dataStream);

            byte[] iv = rsa.DecryptBase64String(dataReader.ReadLine());
            byte[] key = rsa.DecryptBase64String(dataReader.ReadLine());

            return new EncryptionProvider(key, iv);
        }

        /// <summary>
        /// Writes the Key and IV from the specified <see cref="EncryptionProvider"/>
        /// <para/>
        /// to a new <see cref="Packet"/> using the specified <see cref="RSAHelper"/> to encrypt the values
        /// <para/>
        /// this method is the counterpart to <see cref="HandshakeHelper.GetDecryptor(RSAHelper, Packet)"/>
        /// </summary>
        /// <param name="rsa">The <see cref="RSAHelper"/> to use for encrypting the Key and IV</param>
        /// <param name="encryptor">The actual Encryptor to take the Key and IV from</param>
        /// <returns></returns>
        public static Packet WriteEncryptor(RSAHelper rsa, EncryptionProvider encryptor)
        {
            Packet pkt = new Packet();
            MemoryStream outputDataStream = new MemoryStream();
            StreamWriter outputWriter = new StreamWriter(outputDataStream);

            outputWriter.WriteLine(rsa.EncryptToBase64String(encryptor.IV));
            outputWriter.WriteLine(rsa.EncryptToBase64String(encryptor.Key));
            outputWriter.Flush();

            pkt.TypeID = (int)PacketType.Handshake;
            pkt.ChannelID = CryptoCommon.GetPrngInt(); // Adding an element of randomness to our sent data
            pkt.UserID = CryptoCommon.GetPrngInt();// Adding an element of randomness to our sent data
            pkt.Data = outputDataStream.GetBuffer();
            return pkt;
        }

        private static void SendPubkey(StreamWriter dst, RSAHelper helper)
        {
            Packet packet = new Packet((int)PacketType.Handshake);
            packet.TypeID = (int)PacketType.Handshake;
            packet.UserID = CryptoCommon.GetPrngInt(); // Adding an element of randomness to our sent data
            packet.ChannelID = CryptoCommon.GetPrngInt(); // Adding an element of randomness to our sent data
            packet.Data = handshakeEncoding.GetBytes(helper.PublicKey);

            ActualWriteLine(dst, packet);
        }

        private static RSAHelper ReceivePubkey(StreamReader src)
        {
            string read = src.ReadLine();
            Packet rec = ToySerializer.Deserialize<Packet>(Convert.FromBase64String(read));
            if (rec == null)
                throw new Exception("Received data was not a Public Key Packet");
            try
            {
                string pubKey = handshakeEncoding.GetString(rec.Data);
                return new RSAHelper(pubKey);
            }
            catch { throw new Exception("Received Packet Data was not a Public Key"); }
        }
    }
}

