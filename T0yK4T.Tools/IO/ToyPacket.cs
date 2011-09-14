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
    [ProtoContract(Name = "ToyPacket")]
    public sealed class ToyPacket
    {
        /// <summary>
        /// Initializes a new (EMPTY) instance of ToyPacket
        /// <para/>
        /// Please note that the data property of the newly created packet is set to "new byte[0]"
        /// </summary>
        public ToyPacket()
        {
            this.data = new byte[0];
        }

        /// <summary>
        /// Initializes a new instance of ToyPacket and sets the TypeID to the specified value
        /// <para/>
        /// The data property is set to "new byte[0]"
        /// </summary>
        /// <param name="typeID"></param>
        public ToyPacket(uint typeID)
        {
            this.TypeID = typeID;
            this.data = new byte[0];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ToyPacket"/> and sets the TypeID and Data properties to the specified values
        /// </summary>
        /// <param name="typeID"></param>
        /// <param name="data"></param>
        public ToyPacket(uint typeID, byte[] data)
        {
            this.TypeID = typeID;
            this.data = data;
        }

        /// <summary>
        /// Gets or Sets the TypeID contained in this packet
        /// </summary>
        [ProtoMember(1)]
        public uint TypeID { get; set; }

        /// <summary>
        /// Random "padding data"
        /// <para/>
        /// (Generally only used in handshakes for additional protection)
        /// </summary>
        [ProtoMember(2, IsRequired = false)]
        internal int SecInt1
        {
            get;
            set;
        }

        private byte[] data;
        /// <summary>
        /// Gets and Sets the data contained in this packet
        /// </summary>
        [ProtoMember(3)]
        public byte[] Data
        {
            get { return this.data ?? new byte[1]; }
            set { this.data = value ?? new byte[1]; }
        }

        /// <summary>
        /// Random "padding" data
        /// <para/>
        /// (Generally only used in handshakes for additional protection)
        /// </summary>
        [ProtoMember(4, IsRequired = false)]
        internal int SecInt2
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
            byte[] buffer = new byte[16 + this.data.Length];
            int offset = 0;
            
            if (this.SecInt1 == 0)
                this.SecInt1 = T0yK4T.Tools.Cryptography.CryptoCommon.GetPrngInt();
            if (this.SecInt2 == 0)
                this.SecInt2 = T0yK4T.Tools.Cryptography.CryptoCommon.GetPrngInt();

            byte[] tmp = new byte[0];
            
            tmp = BitConverter.GetBytes(this.SecInt1);
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            tmp = BitConverter.GetBytes(this.TypeID);
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            tmp = BitConverter.GetBytes(this.SecInt2);
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            tmp = BitConverter.GetBytes((int)(this.data.Length)); // Writing data size
            Array.Copy(tmp, 0, buffer, offset, tmp.Length);
            offset += tmp.Length;

            Array.Copy(this.data, 0, buffer, offset, this.data.Length);
            return buffer;
        }

        /// <summary>
        /// Fills this instance's fields with values contained in the specified byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void FromBytes(byte[] bytes)
        {
            int offset = 0;

            this.SecInt1 = BitConverter.ToInt32(bytes, offset);
            offset += 4;
            
            this.TypeID = BitConverter.ToUInt32(bytes, offset);
            offset += 4;
            
            this.SecInt2 = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            int dataLength = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            this.data = new byte[dataLength];
            if (this.data.Length > 0)
                Array.Copy(bytes, offset, this.data, 0, this.data.Length);
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
