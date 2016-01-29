using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    [Serializable]
    public class Component
    {
        public string Name { get; set; }
    }
    [Serializable]
    public class StatisticMethod
    {
        public string Name { get; set; }
        public EnumStatistic Method { get; set; }
    }
    [Serializable]
    public class Feature
    {
        public string Name { get; set; }
        public bool IsPerChannel { get; set; }
        public int FeatureIndex { get; set; }
    }
    [Serializable]
    public class Channel {
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
        public string Name { get; set; }
        public List<Component> ComponentContainer { get; set; }
        public List<StatisticMethod> StatisticMethodContainer { get; set; }
        public List<Feature> FeatureContainer { get; set; }
        public List<Channel> ChannelContainer { get; set; }
        public List<CyteRegion> RegionContainer { get; set; }
    }

    [Serializable]
    public class StatisticModel
    {
        public StatisticModel()
        {
            RunFeatureContainer = new List<RunFeature>();
        }
        public List<Component> ComponentContainer { get; set; }
        public List<RunFeature> RunFeatureContainer { get; set; }
        public List<RunFeature> SelectedRunFeature { get; set; }
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
        public float MeanValue{ get; set; }
        public float MedianValue{ get; set; }
        public float CVValue{ get; set; }
    }

    public class WellStatisticEntry
    {
        public string Index { get; set; }
        public string Well { get; set; }
        public string Label { get; set; }
        public string Row { get; set; }
        public string Col { get; set; }
        public float Value { get; set; }
    }
}
