using System;
using System.Linq;

namespace ComponentDataService.Types
{
    public enum FeatureType
    {
        Background, Integral, Intensity, Stdv, MaxPixel, PeripheralIntegral, PeripheralIntensity, PeripheralMax,	// end of per channel
        Area, Circularity, Count, Id, ParentId, Perimeter, Scan, Cycle, Time, WellNo, RegionNo, XPos, YPos, ZPos,
        ArrayXPos, ArrayYPos, SortNo, HalfRadius, Merged, Diameter, MajorAxis, MinorAxis, Elongation, Eccentricity, None
    }

    public class Feature
    {
        #region Fields
        // must be the same order as enumeration, because FeatuerType enum is used as index to the table
        public static readonly FeatureDefinition[] KnownFeatureTable =
        {
            new FeatureDefinition(FeatureType.Background, "Background", true, true, 8),
            new FeatureDefinition(FeatureType.Integral, "Integral", true, false, 8),
            new FeatureDefinition(FeatureType.Intensity, "Intensity", true, true, 8),
            new FeatureDefinition(FeatureType.Stdv, "Stdv", true, true, 8),
            new FeatureDefinition(FeatureType.MaxPixel, "MaxPixel", true, true, 8),
            new FeatureDefinition(FeatureType.PeripheralIntegral, "Peripheral Integral", true, true, 8),
            new FeatureDefinition(FeatureType.PeripheralIntensity, "Peripheral Intensity", true, true, 8), // jcl-7492
            new FeatureDefinition(FeatureType.PeripheralMax, "Peripheral Max", true, true, 8),

            new FeatureDefinition(FeatureType.Area, "Area", false, false, 5),
            new FeatureDefinition(FeatureType.Circularity, "Circularity", false, false, 6),
            new FeatureDefinition(FeatureType.Count, "Count", false, true, 3),
            new FeatureDefinition(FeatureType.Id, "Id", false, true, 4),
            new FeatureDefinition(FeatureType.ParentId, "ParentId", false, true, 4),
            new FeatureDefinition(FeatureType.Perimeter, "Perimeter", false, true, 6),
            new FeatureDefinition(FeatureType.Scan, "Scan", false, true, 3),
            new FeatureDefinition(FeatureType.Cycle, "Cycle", false, true, 3),
            new FeatureDefinition(FeatureType.Time, "Time", false, true, 4),
            new FeatureDefinition(FeatureType.WellNo, "WellNo", false, true, 3),
            new FeatureDefinition(FeatureType.RegionNo, "RegionNo", false, true, 3),
            new FeatureDefinition(FeatureType.XPos, "X Position", false, true, 6),
            new FeatureDefinition(FeatureType.YPos, "Y Position", false, true, 6),
            new FeatureDefinition(FeatureType.ZPos, "Z Position", false, true, 4),
            new FeatureDefinition(FeatureType.ArrayXPos, "ArrayXPos", false, true, 4),
            new FeatureDefinition(FeatureType.ArrayYPos, "ArrayYPos", false, true, 4),
            new FeatureDefinition(FeatureType.SortNo, "SortNo", false, true, 3),
            new FeatureDefinition(FeatureType.HalfRadius, "Radius/2", false, false, 5),
            new FeatureDefinition(FeatureType.Merged, "Merged", false, true, 3),
            new FeatureDefinition(FeatureType.Diameter, "Diameter", false, false, 5),
            new FeatureDefinition(FeatureType.MajorAxis, "Major Axis", false, false, 5),
            new FeatureDefinition(FeatureType.MinorAxis, "Minor Axis", false, false, 5),
            new FeatureDefinition(FeatureType.Elongation, "Elongation", false, false, 6),
            new FeatureDefinition(FeatureType.Eccentricity, "Eccentricity", false, false, 6),
            new FeatureDefinition(FeatureType.None, "(None)", false, false, 0)
        };

        #endregion

        #region Constructors
        public Feature(string name, bool perChannel, bool integer)
		{
            Length = 8;
            FeatureType = FeatureType.None;
            Name = name;
			IsPerChannel = perChannel;
			IsInteger = integer;

			// determine the feature length
			foreach (var def in KnownFeatureTable.Where(def => def.Name == name))
			{
			    Length = def.Length;
			    FeatureType = def.Type;
			    break;
			}
		}

        public Feature(FeatureType f)
		{
			var def = KnownFeatureTable[(int)f];
			Name = def.Name;
			IsPerChannel = def.PerChannel;
			IsInteger = def.IsInteger;
			Length = def.Length;
		    FeatureType = f;
		}
        #endregion

        #region Properties
        public string Name { get; private set; }

        public FeatureType FeatureType { get; private set; }

        public bool IsPerChannel { get; private set; }

        public bool IsInteger { get; private set; }

        public int Length { get; private set; }

        public int Index { get; set; }

        public bool IsSubFeature
        {
            get { return Name.IndexOf("(", StringComparison.Ordinal) >= 0; } //HACK should store property
        }
        #endregion

        #region Methods
        public static string KnownName(FeatureType f)
		{
			var def = KnownFeatureTable[(int)f];
			return def.Name;
		}

        public static string SubFeatureName(FeatureType f, string childComp)
		{
			return string.Format("{0} ({1})", f, childComp);
		}

        public bool IsSubFeatureOf(string child)
        {
            var p1 = Name.IndexOf("(", StringComparison.Ordinal);
            if (p1 <= 0) return false;
            var p2 = Name.IndexOf(")", StringComparison.Ordinal);
            if (p2 <= p1) return false;
            return Name.Substring(p1 + 1, p2 - p1 - 1) == child;
        }

        public override string ToString()
        {
            return Name;
        }
        #endregion

	}
}
