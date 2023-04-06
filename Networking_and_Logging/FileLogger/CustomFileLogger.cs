using Microsoft.Extensions.Logging;

namespace FileLogger
{
    /// <summary>
    /// Author: Mason Sansom
    /// Partner: Druv Rachakonda
    /// Date: 3-Mar-2023
    /// Course:    CS 3500, University of Utah, School of Computing
    /// Copyright: CS 3500 and Mason Sansom - This work may not 
    ///            be copied for use in Academic Coursework.
    ///
    /// We, Mason Sansom and Druve Rachakonda, certify that we wrote this code from scratch and
    /// All references used in the completion of the assignments are cited 
    /// in the README file.
    ///
    /// File Contents
    /// This class is used by CustomFileLogProvider to Log logging 
    /// information to a File
    /// <summary/>
    public class CustomFileLogger : ILogger
    {
        private readonly string categoryName;
        private readonly string _fileName;

        /// <summary>
        ///     This class when called Logs to a custom.log file
        /// </summary>
        /// <param name="categoryName"> name of category </param>
        public CustomFileLogger(string categoryName)
        {
            this.categoryName = categoryName;

             _fileName = Environment.GetFolderPath(
                 Environment.SpecialFolder.Desktop)
                 + Path.DirectorySeparatorChar
                 + $"CS3500-{categoryName}.log;";
        }

        /// <summary>
        ///         Groups messages in the log "stream"
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Creates a log message and logs it to a log file
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string message = $"{DateTime.Now.ToString("yyyy-MM-dd H:mm:ss tt")} ({Thread.CurrentThread.ManagedThreadId}) - {logLevel.ToString().Substring(0, 5)} - {formatter(state, exception)}\n";
            File.AppendAllText(_fileName, message);
        }
    }
}