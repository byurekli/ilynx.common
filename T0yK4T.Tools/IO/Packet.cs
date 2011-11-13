using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
//using ProtoSharp.Core;

namespace T0yK4T.Tools.IO
{
    /// <summary>
    /// The base class for all packets sent and received by ToyChat
    /// </summary>
    [ProtoContract(Name = "Packet")]
    public sealed class Packet
    {
        /// <summary>
        /// Initializes a new (EMPTY) instance of ToyPacket
        /// <para/>
        /// Please note that the data property of the newly created packet is set to "new byte[0]"
        /// </summary>
        public Packet()
        {
            this.data = new byte[0];
        }

        /// <summary>
        /// Initializes a new instance of ToyPacket and sets the TypeID to the specified value
        /// <para/>
        /// The data property is set to "new byte[0]"
        /// </summary>
        /// <param name="typeID"></param>
        public Packet(uint typeID)
        {
            this.TypeID = typeID;
            this.data = new byte[0];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Packet"/> and sets the TypeID and Data properties to the specified values
        /// </summary>
        /// <param name="typeID"></param>
        /// <param name="data"></param>
        public Packet(uint typeID, byte[] data)
        {
            this.TypeID = typeID;
            this.data = data;
        }

        /// <summary>
        /// Gets or Sets the ID of the user that sent this packet
        /// <para/>
        /// NOTE: If this value is set to 0, IT WILL BE RANDOMIZED!
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public int SourceUserID
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or Sets the TypeID contained in this packet
        /// </summary>
        [ProtoMember(2)]
        public uint TypeID { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating which channel this packet is destined for
        /// <para/>
        /// NOTE: If this value is set to 0, IT WILL BE RANDOMIZED!
        /// </summary>
        [ProtoMember(3, IsRequired = true)]
        public int ChannelID
        {
            get;
            set;
        }

        private byte[] data;
        /// <summary>
        /// Gets and Sets the data contained in this packet
        /// </summary>
        [ProtoMember(4)]
        public byte[] Data
        {
            get { return this.data ?? new byte[0]; }
            set { this.data = value ?? new byte[0]; }
        }

        /// <summary>
        /// Gets or Sets a value indicating which user this packet is destined for
        /// <para/>
        /// NOTE: If this value is set to 0, IT WILL BE RANDOMIZED!
        /// </summary>
        [ProtoMember(5, IsRequired = true)]
        public int DestinationUserID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an array of bytes that can be used to represent this instance
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] buffer = new byte[24 + this.data.Length];
            int offset = 0;

            if (this.ChannelID == 0)
                this.ChannelID = T0yK4T.Tools.Cryptography.CryptoCommon.GetPrngInt();
            if (this.DestinationUserID == 0)
                this.DestinationUserID = T0yK4T.Tools.Cryptography.CryptoCommon.GetPrngInt();
            if (this.SourceUserID == 0)
                this.SourceUserID = T0yK4T.Tools.Cryptography.CryptoCommon.GetPrngInt();

            byte[] tmp = new byte[0];

            tmp = BitConverter.GetBytes(this.SourceUserID);
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            tmp = BitConverter.GetBytes(this.TypeID);
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            tmp = BitConverter.GetBytes(this.DestinationUserID);
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            tmp = BitConverter.GetBytes((int)(this.data.Length)); // Writing data size
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            Array.Copy(this.data, 0, buffer, offset, this.data.Length);
            offset += this.data.Length;

            tmp = BitConverter.GetBytes(this.ChannelID);
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            return buffer;
        }

        /// <summary>
        /// Uses the <see cref="ToySerializer"/> to serialize the specified object in to this instance's data field
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="data">The instance of <typeparamref name="T"/> that should be serialized</param>
        public void SerializeData<T>(T data) where T : class, new()
        {
            this.data = ToySerializer.Serialize(data);
        }

        /// <summary>
        /// Attempts to deserialize the data contained in this instance using the <see cref="ToySerializer"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <returns>An instance of <typeparamref name="T"/> created from the data field of this packet</returns>
        public T DeserializeData<T>() where T : class, new()
        {
            try { return ToySerializer.Deserialize<T>(this.data); }
            catch { return null; }
        }

        /// <summary>
        /// Fills this instance's fields with values contained in the specified byte array
        /// </summary>
        /// <param name="bytes"></param>
        public static Packet FromBytes(byte[] bytes)
        {
            Packet ret = new Packet();
            int offset = 0;

            ret.SourceUserID = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            ret.TypeID = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            
            ret.DestinationUserID = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            int dataLength = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            ret.data = new byte[dataLength];
            if (ret.data.Length > 0)
                Array.Copy(bytes, offset, ret.data, 0, ret.data.Length);
            offset += dataLength;

            ret.ChannelID = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            return ret;
        }
    }

    /// <summary>
    /// This enum contains a set of predefined packet types used (among other things) internally
    /// </summary>
    public enum PacketType : uint
    {
        /// <summary>
        /// Defines the TypeID used for handshakes
        /// </summary>
        Handshake = 0,

        /// <summary>
        /// Defines the TypeID used when requesting a renegotiation of Session Keys
        /// </summary>
        HandshakeRequest = 1,

        /// <summary>
        /// Defines the TypeID used when responding to a <see cref="PacketType.HandshakeRequest"/>
        /// <para/>
        /// This TypeID will cause a new handshake if certain requirements are met
        /// </summary>
        InitHandshake = 2,

        /// <summary>
        /// Defines the TypeID used after finishing a full handshake <see cref="PacketType.InitHandshake"/>
        /// </summary>
        EndHandshake = 3,

        /// <summary>
        /// Defines the TypeID used when responding to a <see cref="PacketType.HandshakeRequest"/>
        /// <para/>
        /// This basically has the opposite effect as <see cref="PacketType.InitHandshake"/>
        /// </summary>
        CancelHandshake = 4,

        /// <summary>
        /// Defines the TypeID used when responding to a <see cref="PacketType.HandshakeRequest"/>
        /// <para/>
        /// This TypeID will cause <see cref="CryptoConnection"/> to renegotiate the session key for the requester
        /// </summary>
        InitPartialHandshake = 5,

        /// <summary>
        /// Defines the TypeID used after finishing a partial handshake <see cref="PacketType.InitPartialHandshake"/>
        /// </summary>
        EndPartialHandshake = 6,

        /// <summary>
        /// Defines the TypeID used in <see cref="CryptoConnection"/> when exchanging connection IDs
        /// </summary>
        ConnectionIDExchange = 7,

        /// <summary>
        /// Defines the TypeID used in <see cref="CryptoConnection"/> to signal a "graceful" disconnect
        /// </summary>
        DisconnectNotification = 8,

        /// <summary>
        /// This value is the minimum value of external packet types (this is more of a guideline than a rule)
        /// </summary>
        MIN_EXTERNAL = 250,

        /// <summary>
        /// Thsi value is the maximum value of external packet types (this is more of a guildeline than a rule)
        /// </summary>
        MAX_EXTERNAL = UInt32.MaxValue - 250,

        /// <summary>
        /// Defines the TypeID used when generating ASCII packets with <see cref="PacketFactory.GenerateASCIIPacket(string)"/>
        /// </summary>
        ASCIIPacket = UInt32.MaxValue - 1,

        /// <summary>
        /// Defines the TypeID used when generating Unicode (UTF-8) packets with <see cref="PacketFactory.GenerateUTF8Packet(string)"/>
        /// </summary>
        UnicodePacket = UInt32.MaxValue - 2,
    }
}
