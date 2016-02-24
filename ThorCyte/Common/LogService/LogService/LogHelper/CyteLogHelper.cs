using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;

namespace LogService.LogHelper
{
    public class CyteLogHelper
    {
        #region Fields
        public static event Action<string> OutputMessage;

        private static ILog Loger { get; set; } 

        #endregion

        #region TraceStack
        static CyteLogHelper()
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(@".\XML\log.xml"));
            Loger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
        
        private static string TraceCallStack()
        {
            var st = new StackTrace(true);
            var sb = new StringBuilder();

            //var stkcnt = st.FrameCount > 5 ? 5 : st.FrameCount;
            var stkcnt = st.FrameCount;

            if (stkcnt > 2)
            {
                for (var i = 2; i < stkcnt; i++)
                {
                    var sf = st.GetFrame(i);
                    sb.AppendFormat("File: {0} , Method: {1} , Row: {2} , Coloum: {3}", sf.GetFileName(), sf.GetMethod(),
                        sf.GetFileLineNumber(), sf.GetFileColumnNumber());
                    sb.AppendLine();
                }
            }

            return "\nCALL STACK:\n" + sb;
        }

        private static string TraceCallStack(StackTrace st)
        {
            var sb = new StringBuilder();
            var stkcnt = st.FrameCount;

            if (stkcnt > 2)
            {
                for (var i = 2; i < stkcnt; i++)
                {
                    var sf = st.GetFrame(i);
                    sb.AppendFormat("File: {0} , Method: {1} , Row: {2} , Coloum: {3}", sf.GetFileName(), sf.GetMethod(),
                        sf.GetFileLineNumber(), sf.GetFileColumnNumber());
                    sb.AppendLine();
                }
            }

            return "\nCALL STACK:\n" + sb;
        }
        #endregion

        #region MessageHandle
        private static void HandleMessage(object msg)
        {
            if (OutputMessage != null)
            {
                OutputMessage.Invoke(msg.ToString());
            }
            
        }

        private static void HandleMessage(object msg, Exception ex)
        {
            if (OutputMessage != null)
            {
                OutputMessage.Invoke(string.Format("{0}:{1}", msg, ex.Message));
            }
        }

        private static void HandleMessage(string format, params object[] args)
        {
            if (OutputMessage != null)
            {
                OutputMessage.Invoke(string.Format(format, args));
            }
        }

        #endregion

        #region Log4net functions
        public static void Debug(object message)
        {
            HandleMessage(message);
            if (Loger.IsDebugEnabled)
            {
                Loger.Debug(message);
            }
        }
        public static void Debug(object message, Exception ex)
        {
            HandleMessage(message, ex);
            if (Loger.IsDebugEnabled)
            {
                Loger.Debug(message, ex);
            }
        }
        public static void DebugFormat(string format, params object[] args)
        {
            HandleMessage(format, args);
            if (Loger.IsDebugEnabled)
            {
                Loger.DebugFormat(format, args);
            }
        }
        public static void Error(object message)
        {
            HandleMessage(message);
            if (Loger.IsErrorEnabled)
            {
                Loger.Error(message);
            }
        }
        public static void Error(object message, Exception ex)
        {
            HandleMessage(message, ex);
            if (Loger.IsErrorEnabled)
            {
                Loger.Error(message, ex);
            }
        }
        public static void ErrorFormat(string format, params object[] args)
        {
            HandleMessage(format, args);
            if (Loger.IsErrorEnabled)
            {
                Loger.ErrorFormat(format, args);
            }
        }
        public static void Fatal(object message)
        {
            HandleMessage(message);
            if (Loger.IsFatalEnabled)
            {
                Loger.Fatal(message);
            }
        }
        public static void Fatal(object message, Exception ex)
        {
            HandleMessage(message, ex);
            if (Loger.IsFatalEnabled)
            {
                Loger.Fatal(message, ex);
            }
        }
        public static void FatalFormat(string format, params object[] args)
        {
            HandleMessage(format, args);
            if (Loger.IsFatalEnabled)
            {
                Loger.FatalFormat(format, args);
            }
        }
        public static void Info(object message)
        {
            HandleMessage(message);
            if (Loger.IsInfoEnabled)
            {
                Loger.Info(message);
            }
        }
        public static void Info(object message, Exception ex)
        {
            HandleMessage(message, ex);
            if (Loger.IsInfoEnabled)
            {
                Loger.Info(message, ex);
            }
        }
        public static void InfoFormat(string format, params object[] args)
        {
            HandleMessage(format, args);
            if (Loger.IsInfoEnabled)
            {
                Loger.InfoFormat(format, args);
            }
        }
        public static void Warn(object message)
        {
            HandleMessage(message);
            if (Loger.IsWarnEnabled)
            {
                Loger.Warn(message);
            }
        }
        public static void Warn(object message, Exception ex)
        {
            HandleMessage(message, ex);
            if (Loger.IsWarnEnabled)
            {
                Loger.Warn(message, ex);
            }
        }
        public static void WarnFormat(string format, params object[] args)
        {
            HandleMessage(format, args);
            if (Loger.IsWarnEnabled)
            {
                Loger.WarnFormat(format, args);
            }
        }
        #endregion

        #region Define unhandled Exceptions
        public static void LoadUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Loger.Fatal("UnhandledException", e.ExceptionObject as Exception);
            };
        }
        #endregion
    }
}
