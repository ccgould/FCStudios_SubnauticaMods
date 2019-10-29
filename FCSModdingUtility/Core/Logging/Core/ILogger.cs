namespace FCSModdingUtility
{
    /// <summary>
    /// A logger that will handle log messages from a <see cref="ILogFactory"/>
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Handles the logged message being passed in
        /// </summary>
        /// <param name="message">The message being logged</param>
        /// <param name="level">The level of the log message</param>
        void Log(string message, LogLevel level);
    }
}
