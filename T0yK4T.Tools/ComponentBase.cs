using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace T0yK4T.Tools
{
	/// <summary>
	/// Defines a few methods commonly used throughout ToyChat
	/// </summary>
	public abstract class ComponentBase
	{
		/// <summary>
		/// Contains the logger that is used when logging messages
        /// <para/>
		/// (This MUST be set in order for logging to work (obviously))
        /// <para/>
        /// Will attempt to write to the debug "console" - last resort is stdout (IE. Console.WriteLine)
		/// </summary>
		protected virtual ILogger Logger { get; set; }

        /// <summary>
        /// Initializes the logger of a new instance of <see cref="ComponentBase"/>
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public ComponentBase(ILogger logger) { this.Logger = logger; }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ComponentBase() { }

        #region Logging Methods

		/// <summary>
		/// Writes an error entry to the log
		/// </summary>
		/// <param name="msg">The message of the error</param>
		/// <param name="args">Optional arguments used for <see cref="string.Format(string, object)"/></param>
		public void LogError(string msg, params object[] args)
		{
			this.Log(LoggingType.Error, string.Format(msg, args));
		}

		/// <summary>
		/// Writes a warning entry to the log
		/// </summary>
		/// <param name="msg">The message of the error</param>
        /// <param name="args">Optional arguments used for <see cref="string.Format(string, object)"/></param>
        public void LogWarning(string msg, params object[] args)
		{
			this.Log(LoggingType.Warning, string.Format(msg, args));
		}

        /// <summary>
        /// Writes a critical message to the log
        /// </summary>
        /// <param name="msg">The message to write</param>
        /// <param name="args">Optional arguments used for <see cref="string.Format(string, object)"/></param>
        public void LogCritical(string msg, params object[] args)
        {
            this.Log(LoggingType.Critical, string.Format(msg, args));
        }

		/// <summary>
		/// Writes a debug entry to the log
		/// </summary>
		/// <param name="msg">The message of the error</param>
        /// <param name="args">Optional arguments used for <see cref="string.Format(string, object)"/></param>
        public void LogDebug(string msg, params object[] args)
		{
			this.Log(LoggingType.Debug, string.Format(msg, args));
		}

		/// <summary>
		/// Writes an information entry to the log
		/// </summary>
		/// <param name="msg">The message of the error</param>
        /// <param name="args">Optional arguments used for <see cref="string.Format(string, object)"/></param>
        public void LogInformation(string msg, params object[] args)
		{
			this.Log(LoggingType.Information, string.Format(msg, args));
		}

		/// <summary>
		/// Writes the specified message with the specified type to the log
		/// </summary>
		/// <param name="type">The type of logging message</param>
		/// <param name="msg">The message to write</param>
        public void Log(LoggingType type, string msg)
		{
            if (this.Logger != null)
                this.Logger.Log(type, this, msg);
            else
            {
                string str = string.Format("{0}: {1}", type.ToString(), msg);
                try { Debug.WriteLine(str); }
                catch { Console.WriteLine(str); }
            }
		}

		/// <summary>
		/// Writes a formatted exception message to the log (includes stacktrace and so forth)
		/// </summary>
		/// <param name="er">The exception to log</param>
		/// <param name="method">The method the exception occured in</param>
        public void LogException(Exception er, MethodBase method)
		{
			this.LogError("{4} Caught Exception: {1}{0}Message: {2}{0}StackTrace: {3}", Environment.NewLine, er.ToString(), er.Message, er.StackTrace, method.Name);
		}

		#endregion Logging Methods
	}
}
