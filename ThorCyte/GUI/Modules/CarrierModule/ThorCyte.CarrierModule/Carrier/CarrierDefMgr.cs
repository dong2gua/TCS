using System;
using System.Collections;
using System.Xml;
using ThorCyte.Infrastructure.Exceptions;

namespace ThorCyte.CarrierModule.Carrier
{
    public class CarrierDefMgr
    {
        #region Fileds

        private static CarrierDefMgr _instance;
        private ArrayList _carriers;
        private readonly string _descriptorFilePath;

        #endregion

        #region Constructor

        private CarrierDefMgr(string xmlDir)
        {
            _descriptorFilePath = xmlDir + @"\Descriptor.xml";
            LoadCarriers();
        }

        #endregion

        #region Properties

        public static CarrierDefMgr Instance
        {
            get
            {
                if (_instance == null)
                    throw new CyteException("CarrierDefMgr.Instance", "Attempt to access uninitialized Instance.");
                return _instance;
            }
        }

        public CarrierDef CurrentCarrier { get; private set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            _instance = null;
        }

        public static void Initialize(string xmlDir)
        {
            if (_instance == null)
                _instance = new CarrierDefMgr(xmlDir);
            else
                throw new CyteException("CarrierDefMgr.Initialize", "Already initialized.");
        }

        public ArrayList CarrierDefs
        {
            get { return _carriers; }
        }

        public CarrierDef GetCarrierDef(string idOrName, bool throwException)
        {
            // special case for backward compatibility
            // "96 Well Plastic Plate X Focus" is now "96 Well Plastic Plate"
            if (idOrName == ("96 Well Plastic Plate Top Focus")
                || idOrName == ("96 Well Plastic Plate Bottom Focus"))
            {
                idOrName = "96 Well Plastic Plate";
            }
            CurrentCarrier = null;
            foreach (CarrierDef carrier in _carriers)
            {
                if (carrier.ID == idOrName || carrier.Name == idOrName)
                {
                    CurrentCarrier = carrier;
                    return carrier;
                }
            }

            if (throwException)
                throw new CyteException("CarrierDefMgr.GetCarrierDef", "could not find carrier with ID or name " + idOrName);
            return null;
        }

        private void LoadCarriers()
        {
            _carriers = Load(_descriptorFilePath, true);
        }

        /// <summary>
        /// Load CarrierDefines from XML file.
        /// </summary>
        /// <param name="fileName">XML file name.</param>
        /// <param name="loadStatics">flag if load statics.</param>
        /// <returns>A List of carrier define</returns>
        private static ArrayList Load(string fileName, bool loadStatics)
        {
            var carriers = new ArrayList();
            var doc = new XmlDocument();
            doc.Load(fileName);
            var mp = doc.SelectSingleNode("//microplates");

            //Load the class specific parameters
            if (loadStatics)
                MicroplateDef.LoadStaticsFromXml(mp);

            //Collect all the microplates
            var nodes = doc.SelectNodes("//microplate");
            if (nodes != null)
            {
                foreach (XmlNode n in nodes)
                {
                    //var carrierDef = CarrierDef.CreateFromXml(n); //returns null on error
                    var carrierDef = CreateDefFromXml(n);//returns null on error
                    if (carrierDef != null)
                        carriers.Add(carrierDef);
                }
            }

            //Collect all the slides
            var sl = doc.SelectSingleNode("//slides");
            if (loadStatics)
                SlideDef.LoadStaticsFromXml(sl);

            nodes = doc.SelectNodes("//slide");
            if (nodes != null)
            {
                foreach (XmlNode n in nodes)
                {
                    //var carrierDef = CarrierDef.CreateFromXml(n); //returns null on error
                    var carrierDef = CreateDefFromXml(n);//returns null on error
                    if (carrierDef != null)
                        carriers.Add(carrierDef);
                }
            }
            return carriers;
        }

        /// <summary>
        /// Decide which type of instance will be created according to XML
        /// </summary>
        /// <param name="n">Xml Node</param>
        /// <returns>Return a carrier define object, if error occured return null.</returns>
        private static CarrierDef CreateDefFromXml(XmlNode n)
        {
            CarrierDef carrier = null;	//generic carrier object (could be plate or slide)
            try
            {
                if (n.Name == "slide")
                    carrier = new SlideDef(CarrierDef.CreationType.Empty);		//instantiate as slide
                else
                    carrier = new MicroplateDef(CarrierDef.CreationType.Empty);	//instantiate as microplate

                //Fill up all the values using the correct overloaded method depending on slide or plate
                carrier.LoadFromXml(n);
                return carrier;
            }
            catch (Exception ex)
            {
                //CyteLog.WriteError(ex);
                var msg = string.Format("Error reading carrier type '{0}':\n{1}",
                    (carrier == null) ? "[unknown]" : carrier.Name,
                    ex.Message);
                //CyteMsgBox.Show(MessageType.Error, msg);
                return null;
            }
        }
        #endregion
    }
}
