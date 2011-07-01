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
        public ToyPacket(int typeID)
        {
            this.TypeID = typeID;
            this.data = new byte[0];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ToyPacket"/> and sets the TypeID and Data properties to the specified values
        /// </summary>
        /// <param name="typeID"></param>
        /// <param name="data"></param>
        public ToyPacket(int typeID, byte[] data)
        {
            this.TypeID = typeID;
            this.data = data;
        }

        /// <summary>
        /// Gets or Sets the TypeID contained in this packet
        /// </summary>
        [ProtoMember(1)]//, Name = "TypeID", IsRequired = true)]
        public int TypeID { get; set; }

        /// <summary>
        /// Gets and Sets the UserID this packet belongs to
        /// </summary>
        [ProtoMember(2)]//, Name = "UserID", IsRequired = true)]
        internal int SecInt1
        {
            get;
            set;
        }

        private byte[] data;
        /// <summary>
        /// Gets and Sets the data contained in this packet
        /// </summary>
        [ProtoMember(4)]//, Name = "Data", IsRequired = true)]
        public byte[] Data
        {
            get { return this.data ?? new byte[1]; }
            set { this.data = value ?? new byte[1]; }
        }

        /// <summary>
        /// Gets and Sets the ChannelID this packet belongs to
        /// </summary>
        [ProtoMember(3)]//, Name = "ChannelID", IsRequired = true)]
        internal int SecInt2
        {
            get;
            set;
        }
    }

    /// <summary>
    /// This class contains the "internal" TypeIDs
    /// </summary>
    public enum PredefinedPacketType
    {
        /// <summary>
        /// Defines the TypeID used for handshakes
        /// </summary>
        Handshake = -1,

        /// <summary>
        /// Defines the TypeID used when requesting a renegotiation of Session Keys
        /// </summary>
        HandshakeRequest = -3,

        /// <summary>
        /// Defines the TypeID used when responding to a <see cref="PredefinedPacketType.HandshakeRequest"/>
        /// <para/>
        /// This TypeID will cause a new handshake if certain requirements are met
        /// </summary>
        InitHandshake = -4,

        /// <summary>
        /// Defines the TypeID used after finishing a full handshake <see cref="PredefinedPacketType.InitHandshake"/>
        /// </summary>
        EndHandshake = -5,

        /// <summary>
        /// Defines the TypeID used when responding to a <see cref="PredefinedPacketType.HandshakeRequest"/>
        /// <para/>
        /// This basically has the opposite effect as <see cref="PredefinedPacketType.InitHandshake"/>
        /// </summary>
        CancelHandshake = -6,

        /// <summary>
        /// Defines the TypeID used when responding to a <see cref="PredefinedPacketType.HandshakeRequest"/>
        /// <para/>
        /// This TypeID will cause <see cref="CryptoConnection"/> to renegotiate the session key for the requester
        /// </summary>
        InitPartialHandshake = -7,

        /// <summary>
        /// Defines the TypeID used after finishing a partial handshake <see cref="PredefinedPacketType.InitPartialHandshake"/>
        /// </summary>
        EndPartialHandshake = -8,

        /// <summary>
        /// Defines the TypeID used in <see cref="CryptoConnection"/> when exchanging connection IDs
        /// </summary>
        ConnectionIDExchange = -9,

        /// <summary>
        /// Defines the TypeID used in <see cref="CryptoConnection"/> to signal a "graceful" disconnect
        /// </summary>
        DisconnectNotification = -10,

        /// <summary>
        /// Defines the TypeID used when generating ASCII packets with <see cref="PacketFactory.GenerateASCIIPacket(string)"/>
        /// </summary>
        ASCIIPacket = int.MinValue + 1,

        /// <summary>
        /// Defines the TypeID used when generating Unicode (UTF-8) packets with <see cref="PacketFactory.GenerateUTF8Packet(string)"/>
        /// </summary>
        UnicodePacket = int.MinValue + 2,
    }
}
