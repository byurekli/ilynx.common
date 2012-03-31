using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using ProtoSharp.Core;

namespace T0yK4T.IO
{
    /// <summary>
    /// A simple "factory" that can generate some "non"specific packets
    /// </summary>
    public static class PacketFactory
    {
        /// <summary>
        /// Generates a packet containing the specified string in the Data field
        /// <para/>
        /// ASCII encoded version
        /// </summary>
        /// <param name="message">The message to use</param>
        /// <returns></returns>
        public static Packet GenerateASCIIPacket(string message)
        {
            Packet pkt = new Packet((uint)PacketType.ASCIIPacket);
            pkt.Data = ASCIIEncoding.ASCII.GetBytes(message);
            return pkt;
        }

        /// <summary>
        /// Reads the ASCII string embedded in the <see cref="Packet.Data"/> field in the specified packet
        /// </summary>
        /// <param name="pkt">The packet to "read"</param>
        /// <returns></returns>
        public static string DeGenerateASCIIPacket(Packet pkt)
        {
            if (pkt == null)
                return string.Empty;
            else
            {
                if (pkt.TypeID == (uint)PacketType.ASCIIPacket)
                    return ASCIIEncoding.ASCII.GetString(pkt.Data);
                else
                    return string.Empty;
            }
        }
        
        /// <summary>
        /// Generates a packet containing the specified string in the Data field
        /// <para/>
        /// UTF-8 encoded version
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Packet GenerateUTF8Packet(string message)
        {
            Packet pkt = new Packet((uint)PacketType.UnicodePacket);
            pkt.Data = UnicodeEncoding.UTF8.GetBytes(message);
            return pkt;
            
        }

        /// <summary>
        /// Reads the UTF-8 string embedded in the <see cref="Packet.Data"/> field in the specified packet
        /// </summary>
        /// <param name="pkt">The packet to "read"</param>
        /// <returns></returns>
        public static string DeGenerateUTF8Packet(Packet pkt)
        {
            if (pkt == null)
                return string.Empty;
            else
            {
                if (pkt.TypeID == (uint)PacketType.UnicodePacket)
                    return UnicodeEncoding.UTF8.GetString(pkt.Data);
                else
                    return string.Empty;
            }
        }
    }
}
