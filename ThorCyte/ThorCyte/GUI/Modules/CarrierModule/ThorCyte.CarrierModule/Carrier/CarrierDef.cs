using System;
using System.Collections;
using System.Linq;
using System.Xml;

namespace ThorCyte.CarrierModule.Carrier
{
    public enum CarrierType { None, Slide, Microplate };

    public enum FocusMethodType { None, BottomOnly, BottomTop, TopOnly }

    abstract public class CarrierDef
    {
        public enum CreationType { Empty, NewDefault };	//specify how to initialize a carrier

        public const int UnitFactor = 1000;
        public string ID = string.Empty;		    //A unique identifer (GUID) for a particular carrier
        public string Name = string.Empty;	    //Human readable name of carrier
        public string Owner = string.Empty;	    //ThorCyte or Customer name
        public string Description = string.Empty;	//Human readable description
        public bool CanBeUsedByRobot = false;	//Robot can handle this carrier
        public bool Hidden = false;				//determines whether user can see carrier in lists
        private bool _released = false;          //allows test scans to refine params, but Scan & Save will not run

        /// <summary>
        /// Width for slide, Length for microplate
        /// </summary>
        public abstract int XWidth
        {
            get;
            set;
        }

        /// <summary>
        /// Length for slide, Width for microplate
        /// </summary>
        public abstract int YHeight
        {
            get;
            set;
        }
        public int Thickness = 0;				//Thickness of bottom of microplate or thickness of slide
        public int HeightBaseToWell = 0;		//distance from surface carrier sits on to bottom surface
        public int RetractionOffset = 0;		//amount objective must move to clear
        public FocusMethodType[] AvailableFocusMethods = new FocusMethodType[] { };	//focus methods useable with this carrier
        public FocusMethodType DefaultFocusMethod = FocusMethodType.None;			//one of above, used if not overridden


        /// <summary>
        /// constructor
        /// initializes common members to empty or new default
        /// </summary>
        /// <param name="creationType"></param>
        protected CarrierDef(CreationType creationType)
        {
            switch (creationType)
            {
                case CreationType.NewDefault:
                    //set default value that are different from empty
                    var guid = Guid.NewGuid();
                    ID = guid.ToString();
                    Name = string.Empty;
                    Owner = "Custom";
                    HeightBaseToWell = 3500;
                    Thickness = 1000;
                    AvailableFocusMethods = new FocusMethodType[] { FocusMethodType.BottomOnly };
                    DefaultFocusMethod = FocusMethodType.BottomOnly;
                    break;

                default:
                    break;
            }
        }
        
        public abstract CarrierType Type { get; }

        /// <summary>
        /// One way accessor
        /// Allows reading value
        /// When writing, sets to true regardless of value passed in
        /// </summary>
        public bool Released
        {
            get { return _released; }
            set { _released = true; }
        }

        /// <summary>
        /// display this object as string
        /// </summary>
        public override string ToString() { return Name; }

        public abstract void LoadFromXml(XmlNode carrierNode);
        public abstract XmlNode ToXml(XmlDocument doc);
        protected abstract void ToXml(XmlDocument doc, XmlElement n);
        public abstract CarrierDef Copy();

        public bool IsAvailableFocusMethod(FocusMethodType focusMethod)
        {
            return AvailableFocusMethods.Any(fm => fm == focusMethod);
        }

        protected void LoadBasePropertiesFromXml(XmlNode carrierNode, int unitFactor)
        {
            var attributes = carrierNode.Attributes;
            ID = attributes["id"].InnerText;
            Name = attributes["name"].InnerText;
            Owner = attributes["owner"].InnerText;
            Thickness = (int)(Convert.ToDouble(attributes["thickness"].InnerText) * UnitFactor);

            AvailableFocusMethods = FocusMethods(attributes["available-focus-methods"].InnerText);
            DefaultFocusMethod = FocusMethods(attributes["default-focus-method"].InnerText)[0];
            CanBeUsedByRobot = string.Compare(attributes["robot"].InnerText, "yes", true) == 0;

            RetractionOffset = (int)(Convert.ToDouble(attributes["retraction-offset"].InnerText) * UnitFactor);
            Hidden = string.Compare(attributes["hidden"].InnerText, "yes", true) == 0;
            if (attributes["released"] != null)
            {
                var released = string.Compare(attributes["released"].InnerText, "yes", true) == 0;
                if (released)
                    this.Released = released;   // only set releae, never try to clear
            }
            else
                Released = true;           // if no entry in descriptor set to release

            var descriptionNode = carrierNode.SelectSingleNode("description");
            Description = descriptionNode.InnerText;
            var heightNode = carrierNode.SelectSingleNode("height");
            attributes = heightNode.Attributes;
            HeightBaseToWell = (int)(Convert.ToDouble(attributes["base-to-well"].InnerText) * UnitFactor);

        }

        protected void CopyBase(CarrierDef carrier)
        {
            //copy for new carrier
            carrier.ID = Guid.NewGuid().ToString();
            carrier.Name = "";
            carrier.Owner = "Custom";

            carrier.Description = this.Description;
            carrier.CanBeUsedByRobot = this.CanBeUsedByRobot;
            carrier.Hidden = this.Hidden;

            carrier.Thickness = this.Thickness;
            carrier.HeightBaseToWell = this.HeightBaseToWell;
            carrier.RetractionOffset = this.RetractionOffset;
            carrier.AvailableFocusMethods = (FocusMethodType[])this.AvailableFocusMethods.Clone();
            carrier.DefaultFocusMethod = this.DefaultFocusMethod;
        }

        public FocusMethodType[] FocusMethods(string methodString)
        {
            var methods = new ArrayList();

            var testString = methodString.ToLower();

            for (var i = 0; i < testString.Length - "bottom only".Length + 1; i++)
            {
                if (testString.Substring(i, "bottom only".Length) == "bottom only")
                    methods.Add(FocusMethodType.BottomOnly);
            }
            for (var i = 0; i < testString.Length - "bottom top".Length + 1; i++)
            {
                if (testString.Substring(i, "bottom top".Length) == "bottom top")
                    methods.Add(FocusMethodType.BottomTop);
            }
            for (var i = 0; i < testString.Length - "top only".Length + 1; i++)
            {
                if (testString.Substring(i, "top only".Length) == "top only")
                    methods.Add(FocusMethodType.TopOnly);
            }
            return (FocusMethodType[])methods.ToArray(typeof(FocusMethodType));
        }
    }

}
