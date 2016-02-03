using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Interactivity.InteractionRequest;
using ThorCyte.Statistic.Models;


namespace ThorCyte.Statistic
{
    public class StatisticDataNotification: Confirmation
    {
        public StatisticDataNotification()
        { }
        public StatisticDataNotification(StatisticModel pStatisticModel):this()
        {
            StatisticModelPot = pStatisticModel;
        }

        public StatisticModel StatisticModelPot { get; set; }
        public Component SelectedComponent { get; set; }
        public RunFeature SelectedRunFeature { get; set; }

    }
}
