using System.Windows;
using System.Windows.Controls;
using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Controls
{
    public class GraphicDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ScattergramTemplate { get; set; }

        public DataTemplate HistogramTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var graphiVm = item as ScattergramVm; //CombinedEnity为绑定数据对象
            if (graphiVm != null)
            {
                return ScattergramTemplate;
            }
            return HistogramTemplate;
        }
    }
}
