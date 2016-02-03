using System;
using System.Xml;
using ThorCyte.Infrastructure.Exceptions;

namespace ThorCyte.CarrierModule.Carrier
{
    public class MicroplateDef : CarrierDef
    {
        //plate specific members here...
        public int NumRows = 0;							//Number of well rows
        public int NumColumns = 0;						//Number of well columns
        //public WellShape WellShape = WellShape.Circle;	//rectangle or ellipse
        
        public double WellSize = 0;						//size of well
        public double Interval = 0;						//center to center spacing between wells
        public double MarginTop = 0;						//distance from top edge of plate to center of first row
        public double MarginLeft = 0;						//distance from left edge of plate to center of first column
        private static int sm_xWidth = 0;
        private static int sm_yHeight = 0;


        /// <summary>
        /// standard constructor, creates empty or default microplate
        /// </summary>
        public MicroplateDef(CreationType creationType)
            : base(creationType)
        {
            switch (creationType)
            {
                case CreationType.NewDefault:
                    //set default value that are different from empty
                    NumRows = 8;
                    NumColumns = 12;
                    //WellShape = WellShape.Circle;
                    WellSize = 6.55 * UnitFactor;
                    Interval = 9.0 * UnitFactor;
                    MarginLeft =14.38 * UnitFactor;
                    MarginTop = 11.24 * UnitFactor;
                    break;

                default: break;
            }
        }

        /// <summary>
        /// special constructor for partially defined object (used in Test).
        /// Ideally this should not be needed
        /// </summary>
        public MicroplateDef(string id, string name, int width, int length, int thickness, int rows, int cols, int wellsize, int interval)
        //public MicroplateDef(string id, string name, int width, int length, int thickness, int rows, int cols, int wellsize, int interval, WellShape ws)
            : base(CreationType.Empty)
        {
            ID = id;
            Name = name;
            XWidth = width;
            YHeight = length;
            Thickness = thickness;
            NumRows = rows;
            NumColumns = cols;
            WellSize = wellsize;
            Interval = interval;
            //WellShape = ws;
        }

        public override int XWidth
        {
            get { return sm_xWidth; }
            set { }
        }

        public override int YHeight
        {
            get { return sm_yHeight; }
            set { }
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var n = doc.CreateElement("microplate");
            n.SetAttribute("id", ID.ToString());
            n.SetAttribute("name", Name);
            n.SetAttribute("owner", Owner);
            var child = doc.CreateElement("description");
            child.InnerText = Description;
            n.AppendChild(child);
            n.SetAttribute("thickness", (((double)(Thickness)) / UnitFactor).ToString());
            child = doc.CreateElement("height");
            child.SetAttribute("base-to-well", (((double)(HeightBaseToWell)) / UnitFactor).ToString());
            n.AppendChild(child);

            var afm = string.Empty;
            foreach (var f in AvailableFocusMethods)
            {
                if (afm.Length > 0)
                    afm += ";";
                switch (f)
                {
                    case FocusMethodType.BottomOnly:
                        afm += "Bottom Only";
                        break;
                    case FocusMethodType.BottomTop:
                        afm += "Bottom Top";
                        break;
                    case FocusMethodType.TopOnly:
                        afm += "Top Only";
                        break;
                    default:
                        break;
                }
            }
            n.SetAttribute("available-focus-methods", afm);
            switch (DefaultFocusMethod)
            {
                case FocusMethodType.BottomOnly:
                    n.SetAttribute("default-focus-method", "Bottom Only");
                    break;
                case FocusMethodType.BottomTop:
                    n.SetAttribute("default-focus-method", "Bottom Top");
                    break;
                case FocusMethodType.TopOnly:
                    n.SetAttribute("default-focus-method", "Top Only");
                    break;
                default:
                    break;
            }
            n.SetAttribute("retraction-offset", (((double)(RetractionOffset)) / UnitFactor).ToString());
            n.SetAttribute("robot", CanBeUsedByRobot ? "yes" : "no");
            n.SetAttribute("hidden", Hidden ? "yes" : "no");
            n.SetAttribute("released", Released ? "yes" : "no");

            //let derived classes serialize their members now
            ToXml(doc, n);

            return n;

        }

        /// <summary>
        /// write to Xml
        /// </summary>
        protected override void ToXml(XmlDocument doc, XmlElement n)
        {
            n.SetAttribute("rows", NumRows.ToString());
            n.SetAttribute("columns", NumColumns.ToString());
            n.SetAttribute("wellsize", (WellSize / UnitFactor).ToString());
            n.SetAttribute("interval", (Interval / UnitFactor).ToString());
            //n.SetAttribute("well-shape", WellShape.ToString().ToLower());
            var d = doc.CreateElement("margin");
            d.SetAttribute("left", (MarginLeft / UnitFactor).ToString());
            d.SetAttribute("top", (MarginTop / UnitFactor).ToString());
            var descriptionNode = n.SelectSingleNode("description");
            n.InsertAfter(d, descriptionNode);
        }

        public static void LoadStaticsFromXml(XmlNode mp)
        {
            //scale numerical according to units being uses (mm or inches)
            if (mp.Attributes["unit"].InnerText != "mm")
                throw new CyteException("CarrierDef", "Only units of mm supported.");

            // OK this is fun:
            //  the microplate spec (and the descriptor) calls the longer side Length and the shorter one Width
            //  we call the x dimension width and the y dimension length
            //  hence the xWidth is the microplate's "length", and yHeight is the microplate's "width"
            sm_xWidth = (int)(Convert.ToDouble(mp.Attributes["length"].InnerText) * UnitFactor);
            sm_yHeight = (int)(Convert.ToDouble(mp.Attributes["width"].InnerText) * UnitFactor);
        }

        public override CarrierType Type
        {
            get { return CarrierType.Microplate; }
        }

        public override void LoadFromXml(XmlNode carrierNode)
        {
            base.LoadBasePropertiesFromXml(carrierNode, UnitFactor);

            var attributes = carrierNode.Attributes;
            NumRows = Convert.ToInt32(attributes["rows"].InnerText, 10);
            NumColumns = Convert.ToInt32(attributes["columns"].InnerText, 10);
            //WellShape = (WellShape)Enum.Parse(typeof(WellShape), attributes["well-shape"].InnerText, true);
            WellSize =XmlConvert.ToDouble(attributes["wellsize"].InnerText) * UnitFactor;
            Interval = Convert.ToDouble(attributes["interval"].InnerText) * UnitFactor;
            var marginNode = carrierNode.SelectSingleNode("margin");
            attributes = marginNode.Attributes;
            MarginTop = Convert.ToDouble(attributes["top"].InnerText) * UnitFactor;
            MarginLeft =Convert.ToDouble(attributes["left"].InnerText) * UnitFactor;
        }

        public override CarrierDef Copy()
        {
            var microplate = new MicroplateDef(CreationType.Empty);
            CopyBase(microplate);
            microplate.NumRows = NumRows;
            microplate.NumColumns = NumColumns;
            //microplate.WellShape = WellShape;
            microplate.WellSize = WellSize;
            microplate.Interval = Interval;
            microplate.MarginTop = MarginTop;
            microplate.MarginLeft = MarginLeft;
            return microplate;
        }
    }
}
