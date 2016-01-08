using System;
using System.Collections.Generic;
using System.Xml;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.ViewModels.ModulesBase;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class CombinationModVm : ModuleVmBase
    {
        #region Properties and Fields

        public static string DefaultCategory
        {
            get { return "Combination"; }
        }

        Guid _guid;

        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        string _category;

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        string _createDate;

        public string CreateDate
        {
            get { return _createDate; }
            set { _createDate = value; }
        }

        string _comment;

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        readonly CombinationPortInfo[] _inportInfos = new CombinationPortInfo[3];

        public CombinationPortInfo[] InportInfos
        {
            get { return _inportInfos; }
        }

        readonly CombinationPortInfo[] _outportInfos = new CombinationPortInfo[2];

        public CombinationPortInfo[] OutportInfos
        {
            get { return _outportInfos; }
        }

        List<ModuleVmBase> _subModules = new List<ModuleVmBase>();	// list of sub subModules

        public List<ModuleVmBase> SubModules
        {
            get { return _subModules; }
            set { _subModules = value; }
        }

        int _captionModuleId; // id of a module whose caption string will display on combo module, 0 if none selected

        public int CaptionModuleId
        {
            get { return _captionModuleId; }
            set
            {
                _captionModuleId = value;
            }
        }

        #endregion

        #region Constructors

        public CombinationModVm()
        {
        }

        // create a regular combination module from the specified template
        public CombinationModVm(CombinationModVm template)
        {
        }

        // create a template for combination module
        public CombinationModVm(string name, List<ModuleVmBase> subModules)
        {
            Name = name;
            _subModules = subModules;
            _guid = Guid.NewGuid();
            _createDate = DateTime.Now.ToString("MM-dd-yyyy");
            InitializePorts();
        }

        #endregion

        #region Methods

        private void InitializePorts()
        {

        }

        // create combination module template from file
        public void CreateFromXml(XmlReader reader)
        {
            Id = Convert.ToInt32(reader["id"]);
            Name = reader["name"];
            _category = reader["category"] ?? DefaultCategory;

            _guid = new Guid(reader["guid"]);
            _captionModuleId = CyteConvert.ToInt32(reader["caption-module-id"], 0);

            int modId = CyteConvert.ToInt32(reader["inport1-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                _inportInfos[0] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["inport1-index"], -1);
                //info.DataType = CombinationModVm.ReadPortDataType("inport1-data-Type", reader);
            }

            modId = CyteConvert.ToInt32(reader["inport2-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                _inportInfos[1] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["inport2-index"], -1);
                //info.DataType = CombinationModVm.ReadPortDataType("inport2-data-Type", reader);
            }

            modId = CyteConvert.ToInt32(reader["inport3-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                _inportInfos[2] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["inport3-index"], -1);
                //info.DataType = CombinationModVm.ReadPortDataType("inport3-data-Type", reader);
            }

            modId = CyteConvert.ToInt32(reader["outport1-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                _outportInfos[0] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["outport1-index"], -1);
                //info.DataType = CombinationModVm.ReadPortDataType("outport1-data-Type", reader);
            }

            modId = CyteConvert.ToInt32(reader["outport2-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                _outportInfos[1] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["outport2-index"], -1);
                //info.DataType = CombinationModVm.ReadPortDataType("outport2-data-Type", reader);
            }

            LoadSubModules(reader);
        }


        private void LoadSubModules(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "combination-module") break;

                switch (reader.Name)
                {
                    case "create-date":
                        _createDate = reader.ReadString();
                        break;
                    case "comment":
                        _comment = reader.ReadString();
                        break;
                    case "module":
                        string name = reader["name"];
                        var info = Macro.GetModuleInfo(name);
                        ModuleVmBase module = new CombinationModVm();//(ModuleVmBase)Activator.CreateInstance(Type.GetType(info.Reference, true));
                        if (info != null)
                        {
                            module.Name = info.Name;
                        }
                        module.Id = XmlConvert.ToInt32(reader["id"]);
                        var x = XmlConvert.ToInt32(reader["x"]);
                        var y = XmlConvert.ToInt32(reader["y"]);
                        module.X = x;
                        module.Y = y;
                        module.Deserialize(reader);
                        _subModules.Add(module);
                        break;
                    case "connector":
                        int inId = XmlConvert.ToInt32(reader["inport-module-id"]);
                        int outId = XmlConvert.ToInt32(reader["outport-module-id"]);

                        ModuleVmBase modIn = null;
                        ModuleVmBase modOut = null;
                        // search for the subModules with id
                        foreach (ModuleVmBase m in _subModules)
                        {
                            if (m.Id == outId)
                                modOut = m;
                            else if (m.Id == inId)
                                modIn = m;

                            if (modIn != null && modOut != null)	// both in and out subModules are found
                                break;
                        }

                        int inPortIndex = XmlConvert.ToInt32(reader["inport-index"]);
                        int outPortIndex = Convert.ToInt32(reader["outport-index"]);
                        //new PortConnector(modOut.GetOutPort(outPortIndex), modIn.GetInPort(inPortIndex));
                        break;
                }
            }
        }
        #endregion
    }
}
