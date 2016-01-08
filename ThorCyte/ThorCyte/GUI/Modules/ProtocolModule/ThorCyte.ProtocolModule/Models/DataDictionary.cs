using System.Collections.Generic;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.Models
{
    public class DataDictionary
    {
        #region Properties and Fields

        private static Dictionary<string, ModuleType> _moduleTypeDic = new Dictionary<string, ModuleType>
        {
            {GlobalConst.OutputCateName,ModuleType.SmtOutputCategory},

            {GlobalConst.FilterCateName,ModuleType.SmtFilterCategory},

            {GlobalConst.OperationCateName,ModuleType.SmtOperationCategory},

            {GlobalConst.ContourCateName,ModuleType.SmtContourCategory},
            {GlobalConst.ChannelModuleName,ModuleType.SmtChannelModule},
            {GlobalConst.ThresholdModuleName,ModuleType.SmtThresholdModule},

            {GlobalConst.EventCateName,ModuleType.SmtEventCategory},

            {GlobalConst.ExperimentalCateName,ModuleType.SmtExperimentalCategory},

            {GlobalConst.AdvancedImageAnalysisCateName,ModuleType.SmtAdvancedImageAnalysisCategory},

            {GlobalConst.CustomModulesCateName,ModuleType.SmtCustomModulesCategory}
        };

        public static Dictionary<string, ModuleType> ModuleTypeDic
        {
            get { return _moduleTypeDic; }
        }

        #endregion
    }
}
