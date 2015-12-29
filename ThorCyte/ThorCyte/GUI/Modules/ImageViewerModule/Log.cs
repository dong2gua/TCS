using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace ThorCyte.ImageViewerModule
{
  public  class Log
    {
        private static Log instance;
        public static Log GetInstance()
        {
            if(instance==null)
            {
                instance = new Log();
            }
            return instance;
        }
        private StreamWriter streamWriter;
        private Log()
        {
        }
        public void Write(string str)
        {
            streamWriter = new StreamWriter("log.txt",true);
            streamWriter.Write(str);
            streamWriter.Close();
        }
        public void Clear()
        {
            streamWriter = new StreamWriter("log.txt");
            streamWriter.Write("");
            streamWriter.Close();
        }
    }
}
