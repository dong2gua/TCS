using System;
using LogService.LogHelper;

namespace ThorCyte.Infrastructure.Interfaces
{
    public class ThorCyteLog : ILog
    {
        public void Write(string message)
        {
            CyteLogHelper.Info(message);
        }

        public void Write(string message, Exception ex)
        {
            CyteLogHelper.Error(message, ex);
        }

        public void Write(string message, LogLevel level)
        {
            Action<object> logfunc;
            switch (level)
            {
                case LogLevel.Debug:
                    logfunc = CyteLogHelper.Debug;
                    break;

                case LogLevel.Error:
                    logfunc = CyteLogHelper.Error;
                    break;

                case LogLevel.Fatal:
                    logfunc = CyteLogHelper.Fatal;
                    break;

                case LogLevel.Info:
                    logfunc = CyteLogHelper.Info;
                    break;

                case LogLevel.Warn:
                    logfunc = CyteLogHelper.Warn;
                    break;

                default:
                    logfunc = CyteLogHelper.Info;
                    break;
            }
            logfunc.Invoke(message);
        }


        public void Write(string message, LogLevel level, Exception ex)
        {
            Action<object, Exception> logfunc;
            switch (level)
            {
                case LogLevel.Debug:
                    logfunc = CyteLogHelper.Debug;
                    break;

                case LogLevel.Error:
                    logfunc = CyteLogHelper.Error;
                    break;

                case LogLevel.Fatal:
                    logfunc = CyteLogHelper.Fatal;
                    break;

                case LogLevel.Info:
                    logfunc = CyteLogHelper.Info;
                    break;

                case LogLevel.Warn:
                    logfunc = CyteLogHelper.Warn;
                    break;

                default:
                    logfunc = CyteLogHelper.Info;
                    break;
            }
            logfunc.Invoke(message, ex);
        }
    }
}
