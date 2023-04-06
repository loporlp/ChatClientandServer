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
    /// This class uses the CustomFileLogger to help in logging information
    /// to a File
    /// <summary/>
    public class CustomFileLogProvider : ILoggerProvider
    {
        /// <summary>
        ///   Creates an instance of CustomFileLogger to log to a file
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomFileLogger(categoryName);
        }

        /// <summary>
        ///     Does not need to be implemented as logging is Appended to File
        ///     so the file closes itself
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
