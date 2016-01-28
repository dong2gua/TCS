using System.Collections.Generic;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.Models
{
    public class DataDictionary
    {
        #region Properties and Fields

        private static readonly Dictionary<string, ModuleType> _moduleTypeDic = new Dictionary<string, ModuleType>
        {
            {GlobalConst.Systemstr,ModuleType.SmtSystemCategory},

            {GlobalConst.OutputCateName,ModuleType.SmtOutputCategory},

            {GlobalConst.FilterCateName,ModuleType.SmtFilterCategory},
            {GlobalConst.FilterModuleName,ModuleType.SmtFilterModule},

            {GlobalConst.OperationCateName,ModuleType.SmtOperationCategory},
            {GlobalConst.AddModuleName,ModuleType.SmtAddModule},
            {GlobalConst.SubtractModuleName,ModuleType.SmtSubtractModule},
            {GlobalConst.InvertModuleName,ModuleType.SmtInvertModule},
            {GlobalConst.AndModuleName,ModuleType.SmtAndModule},
            {GlobalConst.OrModuleName,ModuleType.SmtOrModule},
            {GlobalConst.XorModuleName,ModuleType.SmtXorModule},
            {GlobalConst.BrightContrastModuleName,ModuleType.SmtBrightContrastModule},

            {GlobalConst.ContourCateName,ModuleType.SmtContourCategory},
            {GlobalConst.ChannelModuleName,ModuleType.SmtChannelModule},
            {GlobalConst.ThresholdModuleName,ModuleType.SmtThresholdModule},
            {GlobalConst.ContourModuleName,ModuleType.SmtContourModule},

            {GlobalConst.EventCateName,ModuleType.SmtEventCategory},
            {GlobalConst.EventModuleName,ModuleType.SmtEventModule},
            {GlobalConst.PhantomModuleName,ModuleType.SmtPhantomModule},
            {GlobalConst.OverlapParentChildModuleName,ModuleType.SmtOverlapParentChildModule},
            {GlobalConst.OverlapParentChild2ModuleName,ModuleType.SmtOverlapParentChild2Module},

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
