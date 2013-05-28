using System;

namespace T0yK4T.Sound
{
    /// <summary>
    /// Defines an FMOD Exception
    /// </summary>
    public class FMODException : Exception
    {
        private string errorMessage;
        private FMOD.RESULT errorCode;

        /// <summary>
        /// Initializes a new instance of <see cref="FMODException"/> and sets it's ErrorCode to the specified value
        /// <para/>
        /// The message property is set to the string equivalent of <paramref name="errorCode"/>
        /// </summary>
        /// <param name="errorCode">The FMOD.RESULT error code</param>
        public FMODException(FMOD.RESULT errorCode)
        {
            this.errorCode = errorCode;
            this.errorMessage = errorCode.ToString();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FMODException"/> and sets it's fields to the specified values
        /// <para/>
        /// if the <paramref name="message"/> paramter is null or empty, the error code will be used as message instead
        /// </summary>
        /// <param name="errorCode">The FMOD.RESULT error code of this exception</param>
        /// <param name="message">The message of this exception</param>
        public FMODException(string message, FMOD.RESULT errorCode)
        {
            this.errorCode = errorCode;
            this.errorMessage = string.IsNullOrEmpty(message) ? errorCode.ToString() : message;
        }

        /// <summary>
        /// Gets the FMOD.RESULT code contained in this instance
        /// </summary>
        public FMOD.RESULT ErrorCode
        {
            get { return this.errorCode; }
        }

        /// <summary>
        /// Overriden - Implemented own message
        /// </summary>
        public override string Message
        {
            get { return errorMessage; }
        }

        /// <summary>
        /// Overriden to return something a bit more useful
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string part1;
            if (errorMessage == errorCode.ToString())
                part1 = "No Message was specified for this Exception";
            else
                part1 = this.errorMessage;
            return string.Format("{0}{1}Error Code: {2}{1}{3}", part1, Environment.NewLine, this.errorCode, base.ToString());
        }
    }
}