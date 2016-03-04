using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using ThorCyte.Infrastructure.Commom;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class CombinationPortInfo
    {
        public int ModuleId;
        public int PortIndex;
        public PortDataType DataType;

        public CombinationPortInfo()
        {
        }

    }

    public class CombinationModule : ModuleBase
    {
        List<ModuleBase> m_modules = new List<ModuleBase>();	// list of sub modules
        List<ConnectorModel> m_connections = new List<ConnectorModel>();  //list of connections between sub modules 


        Guid m_guid;
        string m_category;
        string m_date;
        string m_comment;
        int m_captionModuleID; // id of a module whose caption string will display on combo module, 0 if none selected
        CombinationPortInfo[] m_inportInfos = new CombinationPortInfo[3];
        CombinationPortInfo[] m_outportInfos = new CombinationPortInfo[2];
        ModuleBase m_outputImageModule; // a module with an image for the output image of combination module
        Hashtable m_mapping = new Hashtable(); // get tab from module

        public CombinationModule()
        {
        }

        // create a regular combination module from the specified template
        public CombinationModule(CombinationModule template)
        {
            CopyFrom(template);
        }

        // create a template for combination module
        public CombinationModule(string name, List<ModuleBase> modules)
        {
            Name = DisplayName = name;
            m_modules = modules;
            m_guid = Guid.NewGuid();
            m_date = DateTime.Now.ToString("MM-dd-yyyy");

            //InitializePorts();
        }

        private void InitializePorts()
        {
            var index = 0;

            foreach (var m in m_modules)   // inports 
            {
                foreach (var p in m.InputPorts.Where(p => p.AttachedConnections.Count == 0 && p.DataType != PortDataType.None))
                {
                    if (index >= 3)
                        throw new CyteException("ProtocolModule.CombinationModule", "Maximum number of input for a combination module is 3.");

                    var info = new CombinationPortInfo();
                    info.ModuleId = m.Id;
                    info.PortIndex = m.InputPorts.IndexOf(p);
                    info.DataType = p.DataType;
                    m_inportInfos[index++] = info;
                }
            }

            index = 0;

            foreach (var m in m_modules)  // outports
            {
                var port = m.GetOutPort();

                if (port != null && port.AttachedConnections.Count == 0 && port.DataType != PortDataType.None)
                {
                    if (index >= 2)
                        throw new CyteException("ProtocolModule.CombinationModule", "Maximum number of output for a combination module is 1.");

                    var info = new CombinationPortInfo();
                    info.ModuleId = m.Id;
                    info.PortIndex = 0;
                    info.DataType = port.DataType;
                    m_outportInfos[index++] = info;
                }
            }
        }

        public static string DefaultCategory
        {
            get { return "Combination"; }
        }

        public Guid Guid
        {
            get { return m_guid; }
            set { m_guid = value; }
        }

        public string Category
        {
            get { return m_category; }
            set { m_category = value; }
        }

        public string CreateDate
        {
            get { return m_date; }
            set { m_date = value; }
        }

        public string Comment
        {
            get { return m_comment; }
            set { m_comment = value; }
        }

        public CombinationPortInfo[] InportInfos
        {
            get { return m_inportInfos; }
        }

        public CombinationPortInfo[] OutportInfos
        {
            get { return m_outportInfos; }
        }

        public List<ModuleBase> SubModules
        {
            get { return m_modules; }
            set { m_modules = value; }
        }

        public List<ConnectorModel> Connectors
        {
            get { return m_connections; }
            set { m_connections = value; }
        }


        public int CaptionModuleId
        {
            get { return m_captionModuleID; }
            set
            {
                m_captionModuleID = value;
            }
        }

        public override object Clone()
        {
            var module = new CombinationModule(this);
            module.Enabled = Enabled;
            module.X = X;
            module.Y = Y;

            return module;
        }

        public IEnumerable<ModuleBase> CloneSubModules()
        {
            var modules = m_modules.Select(m => (ModuleBase)m.Clone()).ToList();
            // copy ids, id is not cloned
            for (var i = 0; i < modules.Count; i++)
            {
                //modules[i].Id = m_modules[i].Id;
                modules[i].ScanNo = m_modules[i].ScanNo; // jcl-7812
            }
            return modules;
        }

        void CopyFrom(CombinationModule temp)
        {
            Id = GetNextModId();
            m_category = temp.Category; // jcl-7359
            Name = DisplayName = temp.DisplayName;
            m_guid = temp.Guid;
            m_date = temp.CreateDate;
            m_comment = temp.Comment;
            m_captionModuleID = temp.CaptionModuleId;
            m_inportInfos = temp.InportInfos;
            m_outportInfos = temp.OutportInfos;

            var modules = temp.CloneSubModules();

            foreach (var m in modules)
            {
                m_modules.Add(m);
            }

            AddPorts();
            AssignHasOutputImage();
        }

        private void AddPorts()
        {
            for (var i = 0; i < m_inportInfos.Length; i++)
            {
                if (m_inportInfos[i] != null)
                {
                    InportInfos[i].DataType = m_inportInfos[i].DataType;
                }
            }

            if (m_outportInfos[0] != null)
            {
                OutputPort.DataType = m_outportInfos[0].DataType;
            }

            //??? two out port?
            //if (m_outportInfos[1] != null)
            //    AddOutPort2(m_outportInfos[1].DataType);
        }


        public static PortDataType ReadPortDataType(string portDataType, XmlReader reader)
        {
            var str = reader[portDataType];
            if (str == null)
                return PortDataType.None;
            else
            {
                if (str == "Blob") // Blob type is obsolete and converted to Event
                    return PortDataType.Event;

                return (PortDataType)Enum.Parse(typeof(PortDataType), str);
            }
        }

        private void AssignHasOutputImage()
        {
            foreach (var m in m_modules)
            {
                if (m.HasImage)
                {
                    HasImage = true;
                    m_outputImageModule = m;
                    break;
                }
            }
        }

        public override void Initialize()
        {
            foreach (var m in m_inportInfos.TakeWhile(info => info != null).Select(info => GetSubModule(info.ModuleId)))
            {
                m.Initialize();
            }

            foreach (var cn in m_outportInfos.TakeWhile(info => info != null).Select(info => GetOutPort()).SelectMany(cp => cp.AttachedConnections))
            {
                cn.DestPort.ParentModule.Initialize();
            }
        }

        public override void OnExecute()
        {
            // transfer data to the sub module and execute network of sub-modules
            for (var i = 0; i < m_inportInfos.Length; i++)
            {
                var info = m_inportInfos[i];
                if (info == null)
                    break;

                var cp = GetInPort(i);   // port on the combination module
                var m = GetSubModule(info.ModuleId);
                var p = m.GetInPort(info.PortIndex); // port on the sub module

                p.Image = cp.Image;
                p.ComponentName = cp.ComponentName;

                m.Execute();
            }

            foreach (var info in m_outportInfos)
            {
                if (info == null)
                    break;

                var cp = GetOutPort();   // port on the combination module
                var m = GetSubModule(info.ModuleId);
                var p = m.GetOutPort(); // port on the sub module

                cp.Image = p.Image;
                cp.ComponentName = p.ComponentName;

                foreach (var cn in cp.AttachedConnections)
                    cn.TransferExecute();
            }
        }

        internal ModuleBase GetSubModule(int id)
        {
            return m_modules.FirstOrDefault(m => m.Id == id);
        }

        // return the inout port on the combination module associated with the specified module id and port index of sub module
        internal PortModel GetComboInPort(int moduleId, int portIndex)
        {
            for (var i = 0; i < m_inportInfos.Length; i++)
            {
                var info = m_inportInfos[i];
                if (info != null && info.ModuleId == moduleId && info.PortIndex == portIndex)
                {
                    return GetInPort(i);
                }
            }

            return null;
        }

        internal PortModel GetComboOutPort(int moduleId, int portIndex)
        {
            for (var i = 0; i < m_outportInfos.Length; i++)
            {
                var info = m_outportInfos[i];
                if (info != null && info.ModuleId == moduleId && info.PortIndex == portIndex)
                {
                    return GetOutPort();
                }
            }

            return null;
        }

        public override bool Executable
        {
            get { return true; }
        }

        public override string CaptionString
        {
            get
            {
                if (m_captionModuleID > 0)
                {
                    foreach (var m in m_modules)
                    {
                        if (m.Id == m_captionModuleID)
                        {
                            return m.CaptionString;
                        }
                    }
                    return string.Empty;
                }
                else
                    return string.Empty;
            }
        }

        // create combination module template from file
        public void CreateFromXml(XmlReader reader)
        {
            Id = Convert.ToInt32(reader["id"]);
            Name = DisplayName = reader["name"];
            m_category = reader["category"];
            if (m_category == null)
                m_category = DefaultCategory; // set default category if not defined

            m_guid = new Guid(reader["guid"]);
            m_captionModuleID = CyteConvert.ToInt32(reader["caption-module-id"], 0);

            var modId = CyteConvert.ToInt32(reader["inport1-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                m_inportInfos[0] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["inport1-index"], -1);
                info.DataType = ReadPortDataType("inport1-data-type", reader);
            }

            modId = CyteConvert.ToInt32(reader["inport2-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                m_inportInfos[1] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["inport2-index"], -1);
                info.DataType = ReadPortDataType("inport2-data-type", reader);
            }

            modId = CyteConvert.ToInt32(reader["inport3-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                m_inportInfos[2] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["inport3-index"], -1);
                info.DataType = ReadPortDataType("inport3-data-type", reader);
            }

            modId = CyteConvert.ToInt32(reader["outport1-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                m_outportInfos[0] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["outport1-index"], -1);
                info.DataType = ReadPortDataType("outport1-data-type", reader);
            }

            modId = CyteConvert.ToInt32(reader["outport2-module-id"], -1);
            if (modId >= 0)
            {
                var info = new CombinationPortInfo();
                m_outportInfos[1] = info;
                info.ModuleId = modId;
                info.PortIndex = CyteConvert.ToInt32(reader["outport2-index"], -1);
                info.DataType = ReadPortDataType("outport2-data-type", reader);
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
                        m_date = reader.ReadString();
                        break;
                    case "comment":
                        m_comment = reader.ReadString();
                        break;
                    case "module":
                        var name = reader["name"];
                        var info = ModuleInfoMgr.GetModuleInfo(name);
                        var module = (ModuleBase)Activator.CreateInstance(Type.GetType(info.Reference, true));
                        module.Name = info.Name;
                        if (!string.IsNullOrEmpty(info.ViewReference))
                        {
                            module.View = (UserControl)Activator.CreateInstance(Type.GetType(info.ViewReference, true));
                        }
                        module.DisplayName = info.DisplayName;
                        module.Id = XmlConvert.ToInt32(reader["id"]);
                        module.Enabled = true;
                        module.ScanNo = Convert.ToInt32(reader["scale"]);
                        module.X = XmlConvert.ToInt32(reader["x"]);
                        module.Y = XmlConvert.ToInt32(reader["y"]);
                        module.Initialize();
                        module.Deserialize(reader);
                        m_modules.Add(module);
                        break;
                    case "connector":
                        var inId = XmlConvert.ToInt32(reader["inport-module-id"]);
                        var outId = XmlConvert.ToInt32(reader["outport-module-id"]);

                        ModuleBase modIn = null;
                        ModuleBase modOut = null;
                        // search for the modules with id
                        foreach (var m in m_modules)
                        {
                            if (m.Id == outId)
                                modOut = m;
                            else if (m.Id == inId)
                                modIn = m;

                            if (modIn != null && modOut != null)	// both in and out modules are found
                                break;
                        }

                        var inPortIndex = XmlConvert.ToInt32(reader["inport-index"]);
                        var outPortIndex = Convert.ToInt32(reader["outport-index"]);
                        m_connections.Add(new ConnectorModel(modOut.OutputPort, modIn.GetInPort(inPortIndex)));
                        break;
                }
            }
        }

        // write combination module to template file
        public void WriteToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement("combination-module");

            writer.WriteAttributeString("id", Id.ToString());
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("category", m_category);
            writer.WriteAttributeString("guid", m_guid.ToString());
            writer.WriteAttributeString("caption-module-id", m_captionModuleID.ToString());

            if (m_inportInfos[0] != null)
            {
                var info = m_inportInfos[0];
                writer.WriteAttributeString("inport1-module-id", info.ModuleId.ToString());
                writer.WriteAttributeString("inport1-index", info.PortIndex.ToString());
                writer.WriteAttributeString("inport1-data-type", info.DataType.ToString());
            }

            if (m_inportInfos[1] != null)
            {
                var info = m_inportInfos[1];
                writer.WriteAttributeString("inport2-module-id", info.ModuleId.ToString());
                writer.WriteAttributeString("inport2-index", info.PortIndex.ToString());
                writer.WriteAttributeString("inport2-data-type", info.DataType.ToString());
            }

            if (m_inportInfos[2] != null)
            {
                var info = m_inportInfos[2];
                writer.WriteAttributeString("inport3-module-id", info.ModuleId.ToString());
                writer.WriteAttributeString("inport3-index", info.PortIndex.ToString());
                writer.WriteAttributeString("inport3-data-type", info.DataType.ToString());
            }

            if (m_outportInfos[0] != null)
            {
                var info = m_outportInfos[0];
                writer.WriteAttributeString("outport1-module-id", info.ModuleId.ToString());
                writer.WriteAttributeString("outport1-index", info.PortIndex.ToString());
                writer.WriteAttributeString("outport1-data-type", info.DataType.ToString());
            }

            if (m_outportInfos[1] != null)
            {
                var info = m_outportInfos[1];
                writer.WriteAttributeString("outport2-module-id", info.ModuleId.ToString());
                writer.WriteAttributeString("outport2-index", info.PortIndex.ToString());
                writer.WriteAttributeString("outport2-data-type", info.DataType.ToString());
            }

            // write sub modules
            WriteSubModules(writer);

            writer.WriteEndElement();	// </combination-module>
        }

        private void WriteSubModules(XmlTextWriter writer)
        {
            writer.WriteStartElement("create-date");
            writer.WriteString(m_date);
            writer.WriteEndElement();	// </create-date>

            writer.WriteStartElement("comment");
            writer.WriteString(m_comment);
            writer.WriteEndElement();	// </comment>

            foreach (var m in m_modules)
            {
                m.Serialize(writer);
            }

            foreach (var c in from m in m_modules from p in m.InputPorts where p.AttachedConnections.Count != 0 from c in p.AttachedConnections select c)
            {
                c.Serialize(writer);
            }
        }
    }
}
