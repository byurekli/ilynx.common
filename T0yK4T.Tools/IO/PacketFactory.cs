using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using ProtoSharp.Core;

namespace T0yK4T.Tools.IO
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
        public static ToyPacket GenerateASCIIPacket(string message)
        {
            ToyPacket pkt = new ToyPacket((int)PredefinedPacketType.ASCIIPacket);
            pkt.Data = ASCIIEncoding.ASCII.GetBytes(message);
            return pkt;
        }

        /// <summary>
        /// Reads the ASCII string embedded in the <see cref="ToyPacket.Data"/> field in the specified packet
        /// </summary>
        /// <param name="pkt">The packet to "read"</param>
        /// <returns></returns>
        public static string DeGenerateASCIIPacket(ToyPacket pkt)
        {
            if (pkt == null)
                return string.Empty;
            else
            {
                if (pkt.TypeID == (int)PredefinedPacketType.ASCIIPacket)
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
        public static ToyPacket GenerateUTF8Packet(string message)
        {
            ToyPacket pkt = new ToyPacket((int)PredefinedPacketType.UnicodePacket);
            pkt.Data = UnicodeEncoding.UTF8.GetBytes(message);
            return pkt;
            
        }

        /// <summary>
        /// Reads the UTF-8 string embedded in the <see cref="ToyPacket.Data"/> field in the specified packet
        /// </summary>
        /// <param name="pkt">The packet to "read"</param>
        /// <returns></returns>
        public static string DeGenerateUTF8Packet(ToyPacket pkt)
        {
            if (pkt == null)
                return string.Empty;
            else
            {
                if (pkt.TypeID == (int)PredefinedPacketType.UnicodePacket)
                    return UnicodeEncoding.UTF8.GetString(pkt.Data);
                else
                    return string.Empty;
            }
        }
    }
}
