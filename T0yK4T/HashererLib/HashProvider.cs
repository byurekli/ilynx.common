using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using T0yK4T.Threading;
using T0yK4T.Tools;

namespace Hasherer
{
    /// <summary>
    /// This class is used to supply either a path to an input file or raw data to a <see cref="AsyncHashProvider"/>
    /// </summary>
    public class HashInputArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="HashInputArgs"/> and sets the <see cref="InputFile"/> property to the specified value
        /// <para/>
        /// <see cref="Mode"/> will be <see cref="HashMode.File"/>
        /// </summary>
        /// <param name="inputFile">The full path to the file to use</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="inputFile"/> parameter is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file specified in <paramref name="inputFile"/> could not be found</exception>
        public HashInputArgs(string inputFile)
        {
            if (string.IsNullOrEmpty(inputFile))
                throw new ArgumentNullException("inputFile");
            if (!File.Exists(inputFile))
                throw new FileNotFoundException("The specified file could not be found", inputFile);
            this.Mode = HashMode.File;
            this.InputFile = inputFile;
        }

        /// <summary>
        /// Initializes a new instancew of <see cref="HashInputArgs"/> and sets the <see cref="RawData"/> property to the specified value
        /// </summary>
        /// <param name="data">The data to use</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null</exception>
        public HashInputArgs(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            this.RawData = data;
            this.Mode = HashMode.Raw;
        }

        /// <summary>
        /// Gets the <see cref="HashMode"/> of these arguments
        /// </summary>
        public HashMode Mode { get; private set; }

        /// <summary>
        /// Gets the full path to the input file of these <see cref="HashInputArgs"/>
        /// <remarks>
        /// Note that this property is only set if <see cref="Mode"/> is <see cref="HashMode.File"/>
        /// </remarks>
        /// </summary>
        public string InputFile { get; private set; }

        /// <summary>
        /// Gets the raw data of these <see cref="HashInputArgs"/>
        /// <remarks>
        /// Note that this property is only set if <see cref="Mode"/> is <see cref="HashMode.Raw"/>
        /// </remarks>
        /// </summary>
        public byte[] RawData { get; private set; }
    }

    /// <summary>
    /// This enum is used in <see cref="HashInputArgs"/> to specify an operation mode for a <see cref="AsyncHashProvider"/>
    /// </summary>
    public enum HashMode
    {
        /// <summary>
        /// Specifies that an input file was given
        /// </summary>
        File,

        /// <summary>
        /// Specifies that raw data was supplied
        /// </summary>
        Raw,
    }

    /// <summary>
    /// Used to return a hash for one execution of a <see cref="AsyncHashProvider"/>
    /// </summary>
    public class HashOutputArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="HashOutputArgs"/> and sets the hash data to the specified value
        /// </summary>
        /// <param name="hash">The hash data</param>
        public HashOutputArgs(byte[] hash)
        {
            this.Hash = hash;
        }

        /// <summary>
        /// Gets the has value that was supplied to these <see cref="HashOutputArgs"/> on creation
        /// </summary>
        public byte[] Hash { get; private set; }
    }

    /// <summary>
    /// This class is used to hash data asynchronously while simultaneously reporting progress of the operation
    /// </summary>
    public class AsyncHashProvider : ProgressWorker<HashInputArgs, HashOutputArgs>
    {
        private HashAlgorithm algorithm;

        /// <summary>
        /// Initializes a new instance of <see cref="AsyncHashProvider"/> using the specified <see cref="HashAlgorithm"/> with the specified name
        /// </summary>
        /// <param name="algorithm">The algorithm to use for hashing data</param>
        /// <param name="algorithmName">The name of the algorithm</param>
        public AsyncHashProvider(HashAlgorithm algorithm, string algorithmName = "")
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");
            this.algorithm = algorithm;
            this.Name = algorithmName;
        }
        
        /// <summary>
        /// Hashes the file or data supplied through the specified <see cref="HashInputArgs"/>
        /// </summary>
        /// <param name="args">The <see cref="HashInputArgs"/> to use for hashing</param>
        /// <returns></returns>
        protected override HashOutputArgs DoWork(HashInputArgs args)
        {
            Stream stream = null;
            try
            {
                switch (args.Mode)
                {
                    case HashMode.File:
                        stream = File.Open(args.InputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                        break;
                    case HashMode.Raw:
                        stream = new MemoryStream(args.RawData);
                        break;
                    default:
                        throw new NotImplementedException("The specified hash mode is not implemented");
                }
                this.HashStream(stream, this.algorithm);
                base.LogInformation("Hash Result: {0}", this.algorithm.Hash.ToString(""));
                return new HashOutputArgs(this.algorithm.Hash);
            }
            finally
            {
                // Cleanup
                if (stream != null)
                    stream.Close();
                this.algorithm.Initialize();
            }
        }

        private void HashStream(Stream inputStream, HashAlgorithm algorithm)
        {
            base.OnProgress(0.0d);
            byte[] chunk = new byte[16384];
            double expectedChunks = Math.Ceiling((double)(inputStream.Length / chunk.Length));
            int read = 0;
            int processedChunks = 0;
            while ((read = inputStream.Read(chunk, 0, chunk.Length)) > 0)
            {
                algorithm.TransformBlock(chunk, 0, read, chunk, 0);
                processedChunks++;
                base.OnProgress((100d / expectedChunks) * processedChunks);
            }
            algorithm.TransformFinalBlock(new byte[0], 0, 0);
            inputStream.Close();
            base.OnProgress(100d);
        }
    }
}
