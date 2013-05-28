

namespace T0yK4T.Tools
{
    /// <summary>
    /// Contains a set of static properties that are considered "Common" inside the T0yK4T.Tools library
    /// </summary>
    public static class RuntimeCommon
    {
        private static LoggingProxy proxy = new LoggingProxy(new ConsoleLogger());

        /// <summary>
        /// Gets a reference to a <see cref="ILogger"/> implementation that is "common" for this runtime
        /// <para/>
        /// This property defaults to the default <see cref="ConsoleLogger"/> implementation of <see cref="ILogger"/>
        /// <remarks>
        /// Note that setting this value will not break the logging functionality for other components,
        /// <para/>
        /// the components that already have a reference to the <see cref="ILogger"/> will merely log to the
        /// <para/>
        /// new <see cref="ILogger"/> implementation
        /// </remarks>
        /// </summary>
        public static ILogger MainLogger
        {
            get { return proxy; }
            set { proxy.Logger = value; }
        }
    }
}