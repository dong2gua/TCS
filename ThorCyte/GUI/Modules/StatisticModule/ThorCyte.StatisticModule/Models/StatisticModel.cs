using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using ComponentDataService;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace ThorCyte.Statistic.Models
{

    public enum EnumStatistic
    { 
        Count,
        Mean,
        Median,
        SD,
        CV,
        Min,
        Max,
        Sum,
        FWHM
    }

    public partial class StatisticMethod
    {
        public static double Count(IEnumerable<float> pData) {
            if(pData == null || pData.Count() == 0)
                return 0;
            return pData.Count();
        }
        public static double Mean(IEnumerable<float> pData) {
            if(pData == null || pData.Count() == 0)
                return 0;
            return pData.Average();
        }
        public static double Median(IEnumerable<float> pData) { 
            if(pData == null || pData.Count() == 0)
                return 0;
            var count = pData.Count();
            var tempData = pData.OrderBy(x => x);
            if (count % 2 == 0)
                return tempData.Skip((count / 2) - 1).Take(2).Average();
            else
                return tempData.Skip(((count - 1) / 2) ).Take(1).ToArray()[0];  
        }
        public static double SD(IEnumerable<float> pData) { 
            if(pData == null || pData.Count() == 0)
                return 0;
            var averageValue = pData.Average();
            var count = pData.Count();
            var squaredSum = pData.Select(x => x * x).Sum();
            return Math.Sqrt(squaredSum / count - averageValue * averageValue);
        }
        public static double CV(IEnumerable<float> pData) { 
            if(pData == null || pData.Count() == 0 || pData.Sum() <= 0)
                return 0;
            return SD(pData) * 100 / Mean(pData);
        }
        public static double Max(IEnumerable<float> pData) {
            if(pData == null || pData.Count() == 0)
                return 0;
            return pData.Max();
        }
        public static double Min(IEnumerable<float> pData) {
            if(pData == null || pData.Count() == 0)
                return 0;
            return pData.Min();
        }
        public static double Sum(IEnumerable<float> pData) {
            if(pData == null || pData.Count() == 0)
                return 0;
            return pData.Sum();
        }
        public static double FWHM(IEnumerable<float> pData) {
            if(pData == null || pData.Count() == 0 || SD(pData) == 0 || Mean(pData) == 0)
				return 0;

            var sdValue = SD(pData);
            var averageValue = Mean(pData);
            var countValue = Count(pData);
            var m_dataList = pData.ToList();

			const int nChannel = 64;
			int[] histogram = new int[nChannel]; 

			// get range, +/- 5 sigma 
			double dMin = averageValue - 5 * sdValue;
			double dMax = averageValue + 5 * sdValue;
			double dInterval = (dMax - dMin) / nChannel;

			// Build histogram
			for(int i = 0; i < countValue; i++)
			{
				double dVal = m_dataList[i];
				if(dVal >= dMin && dVal < dMax)
				{
					histogram[(int)((dVal - dMin) / dInterval)]++;
				}
			}

			// Find peak
			double dHalfPeak = 0;
			int nPeakPos = 0;
			for(int i = 0; i < nChannel; i++)
			{
				if(histogram[i] > dHalfPeak)
				{
					dHalfPeak = histogram[i];
					nPeakPos = i;
				}
			}
			
			if(dHalfPeak < 2)
				return 0;
			dHalfPeak /= 2;

			// The left half-maximum
			int nLeft1 = 0;
			for(int i = 0; i <= nPeakPos; i++)
			{
				if(histogram[i] >= dHalfPeak)
				{
					nLeft1 = i;
					break;
				}
			}

			int nLeft2 = nPeakPos;
			for(int i = nPeakPos; i >= 0 ; i--)
			{
				if(histogram[i] <= dHalfPeak)
				{
					nLeft2 = i;
					break;
				}
			}

			double dLeft = (nLeft1 + nLeft2) / 2.0;

			// The right half-maximum
			int nRight1 = nPeakPos;
			for(int i = nRight1; i < nChannel; i++)
			{
				if(histogram[i] <= dHalfPeak)
				{
					nRight1 = i;
					break;
				}
			}

			int nRight2 = nChannel - 1;
			for(int i = nRight2; i >= nPeakPos ; i--)
			{
				if(histogram[i] >= dHalfPeak)
				{
					nRight2 = i;
					break;
				}
			}

			double dRight = (nRight1 + nRight2) / 2.0;

			// Now we use the formula COV = .425 * FWHM / mean.
			double dfFWHM = (dRight - dLeft) * dInterval;
			return .425 * dfFWHM  / averageValue * 100;
        }

        public static Func<IEnumerable<float>, double> GetStatisticMethod(EnumStatistic pType)
        {
            Func<IEnumerable<float>, double> result;
            switch (pType)
            { 
                case EnumStatistic.Count:
                    result = Count;
                    break;
                case EnumStatistic.Mean:
                    result = Mean;
                    break;
                case EnumStatistic.Median:
                    result = Median;
                    break;
                case EnumStatistic.SD:
                    result = SD;
                    break;
                case EnumStatistic.CV:
                    result = CV;
                    break;
                case EnumStatistic.Max:
                    result = Max;
                    break;
                case EnumStatistic.Min:
                    result = Min;
                    break;
                case EnumStatistic.Sum:
                    result = Sum;
                    break;
                case EnumStatistic.FWHM:
                    result = FWHM;
                    break;
                default:
                    result = Count;
                    break;
            }
            return result;  
        }
    }

    [Serializable]
    public class Component
    {
        public string Name { get; set; }
    }
    [Serializable]
    public partial class StatisticMethod
    {
        public string Name { get; set; }
        public EnumStatistic MethodType { get; set; }
        [XmlIgnore]
        public Func<IEnumerable<float>, double> Method { get; set; }
    }
    [Serializable]
    public class Feature
    {
        public Feature(){
            IsPerChannel = false; 
            FeatureIndex = 0;
        }
        public string Name { get; set; }
        //true for has channel, false get data directly
        public bool IsPerChannel { get; set; }
        public int FeatureIndex { get; set; }
    }
    [Serializable]
    public class Channel {
        public Channel() {
            Index = 0;
        }
        public string Name { get; set; }
        public int Index { get; set; }
    }
    [Serializable]
    public class CyteRegion {
        public string Name { get; set; }
    }

    [Serializable]
    public class RunFeature
    {
        public RunFeature()
        {
            Name = string.Empty;
            IsUserDefineName = false;
        }
        public string Name { get; set; }
        public bool IsUserDefineName { get; set; }
        public List<Component> ComponentContainer { get; set; }
        public List<StatisticMethod> StatisticMethodContainer { get; set; }
        public List<Feature> FeatureContainer { get; set; }
        public List<Channel> ChannelContainer { get; set; }
        public List<CyteRegion> RegionContainer { get; set; }
        public bool IsValid()
        {
            if (ComponentContainer == null || StatisticMethodContainer == null || FeatureContainer == null)
            {
                return false;
            }
            else
            {
                if (ComponentContainer.Count == 0 || StatisticMethodContainer.Count == 0 )
                {
                    return false;
                }
                else
                {
                    if (ComponentContainer[0] == null || StatisticMethodContainer[0] == null)
                    {
                        return false;
                    }
                    else
                    {
                        if (StatisticMethodContainer[0].MethodType == EnumStatistic.Count)
                        {
                            return true;
                        }
                        else if(FeatureContainer.Any() && FeatureContainer[0] != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
        public bool HasChannel()
        {
            if (ChannelContainer == null || ChannelContainer.Count == 0 || ChannelContainer[0] == null)
                return false;
            else
                return true;
        }
        public bool HasRegion()
        { 
            if (RegionContainer == null || RegionContainer.Count == 0 || RegionContainer[0] == null)
                return false;
            else
                return true;
        }
    }

    [Serializable]
    public class StatisticModel
    {
        public StatisticModel()
        {
            RunFeatureContainer = new List<RunFeature>();
        }

        public static string StatisticsPath = "/Statistics";
        public List<ComponentRunFeature> ComponentContainer { get; set; }
        public ComponentRunFeature SelectedComponent { get; set; }
        public List<RunFeature> RunFeatureContainer { get; set; }
        public RunFeature SelectedRunFeature{ get; set; }
        public int ColumnCount { get; set; }

        [Dependency]
        public Func<IEnumerable<IComponentDataService>> ComponentDataFactory { get; set;  }

        private IComponentDataService _componentData;

        public IComponentDataService ComponentData
        {
            get
            {
                if (ComponentDataFactory().Count() > 0)
                    _componentData = ComponentDataFactory().First();
                else
                {
                    _componentData = ComponentDataManager.Instance;
                }
                return _componentData;
            }
        }

       private int GetDataIndex(Feature pFeature, Channel pChannel)
        {
            if (pFeature.IsPerChannel && pChannel != null)
            {
                return pFeature.FeatureIndex + pChannel.Index; 
            }
            else
                return pFeature.FeatureIndex;
        }

        //to do: get featurelist and channellist in advance, the method can be optimized.
        private int GetDataIndex(string pComponentName, string pFeature, string pChannel)
        {
            var featureList = ComponentData.GetFeatures(pComponentName)
                .Select(x => new Feature() { Name = x.Name, IsPerChannel = x.IsPerChannel, FeatureIndex = x.Index }).ToList();
            var feature = featureList.Find(x => x.Name == pFeature);

            Channel channel = null;
            if (pChannel != "")
            {
                var channelList = ComponentData.GetChannels(pComponentName)
                    .Select(x => new Channel() { Name = x.ChannelName }).ToList();
                for (int i = 0; i < channelList.Count; i++)
                {
                    channelList[i].Index = i + 1;
                }
                channel = channelList.Find(x => x.Name == pChannel);
            }

            if (feature != null && channel != null && feature.IsPerChannel)
            {
                return feature.FeatureIndex + channel.Index;
            }
            else if (feature != null)
                return feature.FeatureIndex;
            else
                return 0;
        }


        public List<string> GetColumnGroup()
        {
            var Header = new List<string> {"Index", "Well", "Label", "Row", "Col"};
            RunFeatureContainer.ForEach(x => Header.Add(x.Name));
            return Header;
        }

        public ObservableCollection<ExpandoObject> GetTableData(string ComponentName, int ObjectCount)
        {
            var columncount = 12;
            var num = Enumerable.Range(0, ObjectCount);
                    //to do: get the num from WellList, not create it by yourselft
            var GridViewRowCollection = new ObservableCollection<ExpandoObject>(num.Select(x =>
            {
                //to do: get specific region? where is the API
                var aEvent = ComponentData.GetEvents(ComponentName, x + 1);
                var items = new ExpandoObject() as IDictionary<string, object>;
                items.Add("Index", (x + 1).ToString());
                items.Add("Well",
                    Convert.ToChar((x / columncount + 1) + 64).ToString() + (x % columncount == 0 ? 1 : x % columncount + 1).ToString());
                items.Add("Label", "");
                items.Add("Row", (x / columncount + 1).ToString());
                items.Add("Col", (x % columncount == 0 ? 1 : x % columncount + 1).ToString());

                foreach (var runFeature in RunFeatureContainer)
                {

                    StatisticMethod StatisticMethod = runFeature.StatisticMethodContainer[0];
                    bool IsStatisticCount;
                    Feature CurrentFeature = null;
                    if (StatisticMethod.MethodType == EnumStatistic.Count)
                    {
                        IsStatisticCount = true;
                    }
                    else
                    {
                        IsStatisticCount = false;
                        CurrentFeature = runFeature.FeatureContainer[0];
                    }

                    //to do: add value dynamic
                    if (IsStatisticCount)
                    {
                        items.Add(runFeature.Name,
                            string.Format("{0:f4}", StatisticMethod.Method(aEvent.Select(y => 1.0f))));
                    }
                    else
                    {
                        items.Add(runFeature.Name, string.Format("{0:f4}", StatisticMethod.Method(
                            aEvent.Select(y =>
                                y[
                                    GetDataIndex(CurrentFeature,
                                        runFeature.HasChannel()
                                            ?runFeature.ChannelContainer[0]
                                            : null)]))));
                    }
                }
                return (ExpandoObject)items;
            }));
            return GridViewRowCollection;
        }
        public List<string> GetComponentColumnGroup()
        {
            //to do: add Component Name as Component list   
            var Header = new List<string> {"Index", "Well", "Label", "CN"};
            //RunFeatureContainer.ForEach(x => Header.Add(x.Name));
            Header.Add(RunFeatureContainer[0].Name);
            return Header;
        }

        public ObservableCollection<ExpandoObject> GetComponentTableData(string ComponentName, int ObjectCount)
        {
            var columncount = 12;
            var num = Enumerable.Range(0, ObjectCount);
                    //to do: get the num from WellList, not create it by yourselft
            var GridViewRowCollection = new ObservableCollection<ExpandoObject>(num.SelectMany(x =>
            {
                //to do: get specific region? where is the API
                var aEvent = ComponentData.GetEvents(ComponentName, x + 1);

                int count = 1;
                return aEvent.Select(y =>
                {
                    var items = new ExpandoObject() as IDictionary<string, object>;
                    items.Add("Index", (count++).ToString());
                    items.Add("Well",
                        Convert.ToChar((x/columncount + 1) + 64).ToString() +
                        (x%columncount == 0 ? 1 : x%columncount + 1).ToString());
                    items.Add("Label", "");
                    items.Add("CN", count-1);

                    //foreach (var runFeature in RunFeatureContainer)
                    var runFeature = RunFeatureContainer[0];
                    {
                           var CurrentFeature = runFeature.FeatureContainer[0];
                        items.Add(runFeature.Name, string.Format("{0:f4}",
                            y[
                                GetDataIndex(CurrentFeature,
                                    runFeature.HasChannel()
                                    ? runFeature.ChannelContainer[0]:null)]));
                    }
                    return (ExpandoObject) items;
                });
            }));
            return GridViewRowCollection;
        }
    }
    
    public class ComponentRunFeature
    {
        public ComponentRunFeature()
        {
            CurrentComponent = null;
            RunFeatureContainer = null;
        }
        public Component CurrentComponent { get; set; }
        public List<RunFeature> RunFeatureContainer { get; set; }
    }

    public class RegionStatisticEntry  
    {
        public string RegionName{ get; set; }
        public string Label{ get; set; }
        public string Parameter { get; set; }
        public double MeanValue{ get; set; }
        public double MedianValue{ get; set; }
        public double CVValue{ get; set; }
    }

    public class WellStatisticEntry
    {
        public string Index { get; set; }
        public string Well { get; set; }
        public string Label { get; set; }
        public string Row { get; set; }
        public string Col { get; set; }
        public List<float> Value { get; set; }
    }

}
