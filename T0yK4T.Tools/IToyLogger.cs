using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace T0yK4T.Tools
{
	/// <summary>
	/// an Interface that should be used for logging purposes
	/// </summary>
	public interface IToyLogger
	{
		/// <summary>
		/// Writes a line to the log (Whichever that would be)
		/// </summary>
		/// <param name="type">The Type of logging</param>
		/// <param name="sender">Should contain the sending object</param>
		/// <param name="message">The message to post to the log</param>
		void Log(LoggingType type, object sender, string message);
	}
}
