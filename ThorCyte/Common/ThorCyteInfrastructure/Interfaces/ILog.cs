using System;

namespace ThorCyte.Infrastructure.Interfaces
{
    public interface ILog
    {
        //Write message into log file default log level is Info.
        void Write(string message);
        //Write error into log file default log level is Error.
        void Write(string message, Exception ex);
        void Write(string message, LogLevel level);
        void Write(string message, LogLevel level, Exception ex);
    }

    public enum LogLevel
    {
        Debug,
        Error,
        Fatal,
        Info,
        Warn
    }
}
