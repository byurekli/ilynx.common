using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using T0yK4T.Tools.General;

namespace T0yK4T.Tools
{
    /// <summary>
    /// Simple Console Logger - will dump anything to console
    /// </summary>
    public class ConsoleLogger : IToyLogger
    {
        /// <summary>
        /// Writes the specified message to the console
        /// </summary>
        /// <param name="type">The type of message</param>
        /// <param name="sender">The sender of the message</param>
        /// <param name="message">The message itself</param>
        public void Log(LoggingType type, object sender, string message)
        {
            Console.WriteLine("[{0}:{1}]: {2}", type.ToString()[0], sender.GetType().FullName, message);
        }
    }
}
