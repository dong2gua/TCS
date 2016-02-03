using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThorCyte.Infrastructure.Exceptions
{
    public class CyteException : ApplicationException
    {

        #region Fileds

        private string _threadName;
        private readonly string _message;

        #endregion

        public CyteException(string src, string msg)
        {
            Source = src;
           _threadName = Thread.CurrentThread.Name;
            _message = msg;
        }

        public override string Message
        {
            get { return _message; }
        }
    }
}
