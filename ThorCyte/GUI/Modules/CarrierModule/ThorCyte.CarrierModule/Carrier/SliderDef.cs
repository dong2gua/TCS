using System;
using System.Collections;
using System.Xml;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule.Carrier
{
    public class SlideDef : CarrierDef
    {
        //freeform specific members here...
        public ArrayList Rooms = new ArrayList();	//list of rooms (for multi-slide & petri dish etc.)
        public int MarginTop = 0;		//distance from top of adapter to top of slide
        public int MarginRight = 0;		//distance from right edge of adapter to right edge of slide
        private int m_xWidth = 0;
        private int m_yHeight = 0;

        /// <summary>
        /// standard constructor, creates empty or default slide
        /// </summary>
        public SlideDef(CreationType creationType)
            : base(creationType)
        {
            switch (creationType)
            {
                case CreationType.NewDefault:
                    //set default value that are different from empty
                    m_xWidth = Convert.ToInt32(127.76 * UnitFactor);
                    m_yHeight = Convert.ToInt32(85.48 * UnitFactor);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// special constructor for partially defined object (used in Test).
        /// Ideally this should not be needed
        /// </summary>
        public SlideDef(string id, string name, int width, int length, int thickness)
            : base(CreationType.Empty)
        {
            ID = id;
            Name = name;
            XWidth = width;
            YHeight = length;
            Thickness = thickness;
        }

        public override int XWidth
        {
            get { return m_xWidth; }
            set { m_xWidth = value; }
        }

        public override int YHeight
        {
            get { return m_yHeight; }
            set { m_yHeight = value; }
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            var n = doc.CreateElement("slide");
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
            n.SetAttribute("width", (((double)(XWidth)) / UnitFactor).ToString());
            n.SetAttribute("length", (((double)(YHeight)) / UnitFactor).ToString());

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
            var d = doc.CreateElement("margin");
            d.SetAttribute("right", (((double)(MarginRight)) / UnitFactor).ToString());
            d.SetAttribute("top", (((double)(MarginTop)) / UnitFactor).ToString());
            var descriptionNode = n.SelectSingleNode("description");
            n.InsertAfter(d, descriptionNode);
            if (Rooms.Count <= 0) return;

            d = doc.CreateElement("rooms");
            n.AppendChild(d);
            foreach (RoomDef room in Rooms)
            {
                var r = doc.CreateElement("room");
                r.SetAttribute("no", room.Number.ToString());
                r.SetAttribute("shape", room.Shape == RegionShape.Rectangle ? "rectangle" : "circle");
                r.SetAttribute("x", (((double)(room.rect.X)) / UnitFactor).ToString());
                r.SetAttribute("y", (((double)(room.rect.Y)) / UnitFactor).ToString());
                r.SetAttribute("w", (((double)(room.rect.Width)) / UnitFactor).ToString());
                r.SetAttribute("h", (((double)(room.rect.Height)) / UnitFactor).ToString());
                d.AppendChild(r);
            }
        }

        public static void LoadStaticsFromXml(XmlNode mp)
        {
            //scale numerical according to units being uses (mm or inches)
            if (mp.Attributes != null && mp.Attributes["unit"].InnerText != "mm")
                throw new CyteException("CarrierDef", "Only units of mm supported."); //Marked by zhwang due to Cyte Exception did not implement.
        }

        public override CarrierType Type
        {
            get { return CarrierType.Slide; }
        }

        public override void LoadFromXml(XmlNode carrierNode)
        {
            base.LoadBasePropertiesFromXml(carrierNode, UnitFactor);

            m_xWidth = (int)(Convert.ToDouble(carrierNode.Attributes["width"].InnerText) * UnitFactor);
            m_yHeight = (int)(Convert.ToDouble(carrierNode.Attributes["length"].InnerText) * UnitFactor);
            var marginNode = carrierNode.SelectSingleNode("margin");
            var attributes = marginNode.Attributes;
            this.MarginTop = (int)(Convert.ToDouble(attributes["top"].InnerText) * UnitFactor);
            this.MarginRight = (int)(Convert.ToDouble(attributes["right"].InnerText) * UnitFactor);

            var roomsNode = carrierNode.SelectSingleNode("rooms");
            if (roomsNode == null) return;

            foreach (XmlNode roomNode in roomsNode.ChildNodes)
            {
                var room = new RoomDef();
                room.LoadFromXml(roomNode, UnitFactor);
                Rooms.Add(room);
            }
        }

        public override CarrierDef Copy()
        {
            var slide = new SlideDef(CreationType.Empty);
            CopyBase(slide);
            slide.XWidth = XWidth;
            slide.YHeight = YHeight;
            slide.MarginTop = MarginTop;
            slide.MarginRight = MarginRight;

            foreach (RoomDef room in Rooms)
            {
                slide.Rooms.Add(room.Copy());
            }

            return slide;
        }
    }
}
