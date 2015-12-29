using System;
using System.Windows;
using System.Xml;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule.Carrier
{
    /// <summary>
    /// Define the rooms on the slider carrier
    /// </summary>
    public class RoomDef
    {
        public int Number;
        public RegionShape Shape;
        public Rect rect;

        public RoomDef()
        {
        }

        public RoomDef Copy()
        {
            var room = new RoomDef
            {
                Number = Number,
                Shape = Shape,
                rect = rect
            };

            return room;
        }

        public void LoadFromXml(XmlNode roomNode, int unitFactor)
        {
            var attributes = roomNode.Attributes;
            Number = Convert.ToInt32(attributes["no"].InnerText, 10);

            Shape = RegionShape.Rectangle;
            if (string.Compare(attributes["shape"].InnerText, "rectangle", StringComparison.OrdinalIgnoreCase) != 0)
                Shape = RegionShape.Ellipse;

            //NOTE: the x, y values denote the "top-right" corner of the room
            //do NOT use rect.left or rect.right in any calculations!!
            var x = Convert.ToInt32(XmlConvert.ToDouble(attributes["x"].InnerText) * unitFactor);
            var y = Convert.ToInt32(XmlConvert.ToDouble(attributes["y"].InnerText) * unitFactor);
            var w = Convert.ToInt32(XmlConvert.ToDouble(attributes["w"].InnerText) * unitFactor);
            var h = Convert.ToInt32(XmlConvert.ToDouble(attributes["h"].InnerText) * unitFactor);
            rect = new Rect(x, y, w, h);
        }

        public override string ToString()
        {
            return "R" + Number;
        }

    }
}
