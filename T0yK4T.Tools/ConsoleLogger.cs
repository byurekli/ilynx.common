using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using T0yK4T.Tools.General;
using System.IO;

namespace T0yK4T.Tools
{
    /// <summary>
    /// Simple Console Logger - will dump anything to console
    /// </summary>
    public class ConsoleLogger : ILogger, IDisposable
    {
        private Stream os;
        private StreamWriter writer;

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public ConsoleLogger()
        {
            if (Console.LargestWindowHeight > 50 && Console.LargestWindowWidth > 50)
                Console.SetWindowSize((int)(Console.LargestWindowWidth / 1.5), (int)(Console.LargestWindowHeight / 1.5));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConsoleLogger"/> and optionally logs to a file
        /// </summary>
        /// <param name="dumpFile"></param>
        public ConsoleLogger(string dumpFile)
        {
            string ext = Path.GetExtension(dumpFile);
            if (string.IsNullOrEmpty(ext))
                ext = "log";

            int p2 = 0;
            while (File.Exists(Path.Combine(Environment.CurrentDirectory, string.Format("{0}{1}{2}", dumpFile, p2, ext))))
                p2++;
            os = File.Create(Path.Combine(Environment.CurrentDirectory, string.Format("{0}{1}{2}", dumpFile, p2, ext)));
            writer = new StreamWriter(os);
        }

        /// <summary>
        /// Writes the specified message to the console
        /// </summary>
        /// <param name="type">The type of message</param>
        /// <param name="sender">The sender of the message</param>
        /// <param name="message">The message itself</param>
        public void Log(LoggingType type, object sender, string message)
        {
            string line = string.Format("[{0}:{1}]: {2}", type.ToString()[0], sender.GetType().FullName, message);
            Console.WriteLine(line);
            if (writer != null)
            {
                writer.WriteLine(line);
                writer.Flush();
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ConsoleLogger()
        {
            try
            {
                if (this.writer != null)
                    this.writer.Close();
            }
            catch { }
            finally
            {
                this.os = null;
                this.writer = null;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.writer != null)
                this.writer.Dispose();
            if (this.os != null)
                this.os.Dispose();
        }

        #endregion
    }
}
