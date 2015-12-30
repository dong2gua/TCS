using System.Collections.Generic;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.Models
{
    public class DataDictionary
    {
        #region Properties and Fields

        private static Dictionary<string, ModuleType> _moduleTypeDic = new Dictionary<string, ModuleType>
        {
            {GlobalConst.ImageViewName,ModuleType.SmtOutputImageVIew},
            {GlobalConst.FilterName,ModuleType.SmtFilter},
            {GlobalConst.ChannelName,ModuleType.SmtContourChannel},
            {GlobalConst.PmtName,ModuleType.SmtScanDetectors},
            {GlobalConst.FieldScanName,ModuleType.SmtFieldScan}
        };

        public static Dictionary<string, ModuleType> ModuleTypeDic
        {
            get { return _moduleTypeDic; }
        }

        #endregion
    }
}
