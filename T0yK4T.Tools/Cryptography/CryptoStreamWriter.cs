using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace T0yK4T.Tools.Cryptography
{
    /// <summary>
    /// Class that can be used to write strings and other stuff to a <see cref="CryptoStream"/>
    /// <para/>
    /// Derrives from <see cref="StreamWriter"/>
    /// </summary>
    public class CryptoStreamWriter : StreamWriter
    {
        /// <summary>
        /// Our stream...
        /// </summary>
        private CryptoStream m_Stream;

        /// <summary>
        /// Initializes a new instance of CryptoStreamWriter and sets the internal stream to the specified value
        /// </summary>
        /// <param name="stream"></param>
        public CryptoStreamWriter(CryptoStream stream)
            : base(stream)
        {
            this.m_Stream = stream;
        }

        /// <summary>
        /// Overriden to call <see cref="CryptoStream.FlushFinalBlock"/> as well
        /// </summary>
        public override void Flush()
        {
            base.Flush();
            m_Stream.FlushFinalBlock();
        }
    }
}
