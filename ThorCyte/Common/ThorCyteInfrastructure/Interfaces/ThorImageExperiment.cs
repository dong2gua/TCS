using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.Infrastructure.Interfaces
{
    public class ThorImageExperiment : IExperiment
    {
        #region Filed
        private IList<ScanInfo> _scanInfos;
        private ExperimentInfo _experimentInfo;
        private ScanInfo _currentScanInfo;
        private string _carrierType;
        private int _captureMode;
        private bool _zTStream;

        private readonly List<string> _carrierList = new List<string> { 
            "00000000-0000-0000-0001-000000000008",  //WELL6
            "00000000-0000-0000-0001-000000000006",  //WELL24
            "00000000-0000-0000-0001-000000000003",  //WELL96
            "00000000-0000-0000-0001-000000000009",  //WELL384
            "00000000-0000-0000-0001-00000000000b",  //WELL1536 
            "00000000-0000-0000-0000-000000000001"   //SLIDE
        };
        #endregion

        public bool Load(string experimentPath)
        {
            try
            {
                _scanInfos = new List<ScanInfo>();
                _currentScanInfo = new ScanInfo();
                _scanInfos.Add(_currentScanInfo);
                _experimentInfo = new ExperimentInfo();
                _experimentInfo.InstrumentType = "ThorImage";
                _experimentInfo.IntensityBits = 14;
                _currentScanInfo.DataPath = Path.GetDirectoryName(experimentPath);
                _experimentInfo.ExperimentPath = _currentScanInfo.DataPath;
                _experimentInfo.AnalysisPath = "";
                _experimentInfo.LoadWithAnalysisResult = false;
                _currentScanInfo.ScanId = 1;
                _currentScanInfo.ResolutionUnit = ResUnit.Micron;
                _currentScanInfo.ScanPathMode = ScanPathType.Serpentine;
                XmlReader reader = new XmlTextReader(experimentPath);
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        ProcessElement(reader);
                }

                SetCaptureMode(_currentScanInfo);
                SetFileMode(_currentScanInfo);
                foreach (var sr in _currentScanInfo.ScanRegionList)
                {
                    sr.BulidTiles(_currentScanInfo.XPixcelSize*_currentScanInfo.TileWidth,
                        _currentScanInfo.YPixcelSize*_currentScanInfo.TiledHeight, 0, 0, ScanPathType.Serpentine);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public ExperimentInfo GetExperimentInfo()
        {
            return _experimentInfo;
        }

        public int GetScanCount()
        {
            if (_scanInfos != null)
                return _scanInfos.Count;
            return 0;
        }

        public ScanInfo GetScanInfo(int scanId)
        {
            if (scanId > 0 && scanId <= _scanInfos.Count)
                return _scanInfos[scanId - 1];
            return null;
        }

        public string GetCarrierType()
        {
            return _carrierType;
        }

        public BitmapSource GetCameraView()
        {
            return null;
        }

        public int GetCurrentScanId()
        {
            return 1;
        }

        public void SetAnalysisPath(string path, bool isLoad)
        {
            if (Directory.Exists(path))
            {
                _experimentInfo.AnalysisPath = path;
                if (isLoad)
                    _experimentInfo.LoadWithAnalysisResult = true;
            } 
        }

        private void ProcessElement(XmlReader reader)
        {
            switch (reader.Name)
            {
                case "Name":
                    _experimentInfo.Name = reader["name"];
                    break;
                case "Date":
                    _experimentInfo.Date = reader["date"];
                    break;
                case "User":
                    _experimentInfo.UserName = reader["name"];
                    break;
                case "Computer":
                    _experimentInfo.ComputerName = reader["name"];
                    break;
                case "Software":
                    _experimentInfo.SoftwareVersion = reader["version"];
                    break;
                case "Magnification":
                    _currentScanInfo.ObjectiveType = reader["name"];
                    break;
                case "Wavelengths":
                    LoadChannels(reader);
                    break;
                case "ZStage":
                    _zTStream = XmlConvert.ToBoolean(reader["zStreamMode"]);
                    if (_zTStream)
                    {
                        _currentScanInfo.StreamFrameCount = XmlConvert.ToInt32(reader["zStreamFrames"]);
                    }
                    else
                    {
                        _currentScanInfo.StreamFrameCount = 0;
                    }
                    _currentScanInfo.ThirdDimensionSteps = XmlConvert.ToInt32(reader["steps"]);
                    _currentScanInfo.ZPixcelSize = XmlConvert.ToDouble(reader["stepSizeUM"]);
                    break;
                case "Timelapse":
                    _currentScanInfo.TimingFrameCount = XmlConvert.ToInt32(reader["timepoints"]);
                    _currentScanInfo.TimeInterval = XmlConvert.ToDouble(reader["intervalSec"]);
                    break;
                case "Sample":
                    LoadScanRegion(reader);
                    break;
                case "LSM":
                    _currentScanInfo.TiledHeight = XmlConvert.ToInt32(reader["pixelY"]);
                    _currentScanInfo.TileWidth = XmlConvert.ToInt32(reader["pixelX"]);

                    double lsmWidth = XmlConvert.ToDouble(reader["widthUM"]);
                    double lsmHeight = XmlConvert.ToDouble(reader["heightUM"]);

                    _currentScanInfo.XPixcelSize = lsmWidth / _currentScanInfo.TileWidth;
                    _currentScanInfo.YPixcelSize = lsmHeight / _currentScanInfo.TiledHeight;
                    break;
                case "Streaming":
                    int enable = XmlConvert.ToInt32(reader["enable"]);
                    if (enable == 1)
                    {
                        _currentScanInfo.StreamFrameCount = XmlConvert.ToInt32(reader["frames"]);
                        
                        int fastZEnable = XmlConvert.ToInt32(reader["zFastEnable"]);
                        if (fastZEnable == 1)
                        {
                            _currentScanInfo.FlybackFrameCount = XmlConvert.ToInt32(reader["flybackFrames"]);
                            _currentScanInfo.StreamFrameCount /= (_currentScanInfo.FlybackFrameCount + _currentScanInfo.ThirdDimensionSteps);
                        }
                        else
                        {
                            _currentScanInfo.FlybackFrameCount = 0;
                        }
                    }
                    else
                    {
                        if (!_zTStream)
                            _currentScanInfo.StreamFrameCount = 0;
                    }

                    break;
                case "CaptureMode":
                    _captureMode = XmlConvert.ToInt32(reader["mode"]);
                    if (_captureMode == 1)
                    {
                        _currentScanInfo.TimingFrameCount = 0;
                    }
                    break;
                case "ExperimentNotes":
                    _experimentInfo.Notes = reader["text"];
                    break;
            }
        }

        private void LoadChannels(XmlReader reader)
        {
            IList<Channel> channels = _currentScanInfo.ChannelList;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Wavelength")
                {
                    Channel channel = new Channel();
                    channel.ChannelName = reader["name"];
                    channel.ChannelId = channels.Count;
                    channels.Add(channel);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Wavelengths")
                    break;
            }
        }

        private void LoadScanRegion(XmlReader reader)
        {
            int carrierId = XmlConvert.ToInt16(reader["type"]);
            _carrierType = _carrierList[carrierId];

            if (carrierId == 5)
            {
                LoadSlide(reader);
            }
            else
            {
                LoadMicroplate(reader);
            }
        }

        private void LoadSlide(XmlReader reader)
        {
            int regionId = 0;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Wells":

                            break;
                        case "SubImages":

                            double transOffsetX = XmlConvert.ToDouble(reader["transOffsetXMM"]);
                            double transOffsetY = XmlConvert.ToDouble(reader["transOffsetYMM"]);

                            int subRows = XmlConvert.ToInt16(reader["subRows"]);
                            int subColumns = XmlConvert.ToInt16(reader["subColumns"]);

                            double subOffsetX = Math.Round(XmlConvert.ToDouble(reader["subOffsetXMM"]), 5);
                            double subOffsetY = Math.Round(XmlConvert.ToDouble(reader["subOffsetYMM"]), 5);

                            double hScanRegionHeight = subRows * subOffsetY * 500;
                            double hScanRegionWidth = subColumns * subOffsetX * 500;

                            double wellCenterX = (37.5 + transOffsetX) * 1000;
                            double wellCenterY = (12.5 + transOffsetY) * 1000;

                            Point[] ptList = new Point[4];
                            ptList[0].X = wellCenterX - hScanRegionWidth;
                            ptList[0].Y = wellCenterY - hScanRegionHeight;
                            ptList[1].X = wellCenterX + hScanRegionWidth;
                            ptList[1].Y = wellCenterY - hScanRegionHeight;
                            ptList[2].X = wellCenterX - hScanRegionWidth;
                            ptList[2].Y = wellCenterY + hScanRegionHeight;
                            ptList[3].X = wellCenterX + hScanRegionWidth;
                            ptList[3].Y = wellCenterY + hScanRegionHeight;

                            ScanRegion sr = new ScanRegion(regionId + 1, regionId++, RegionShape.Rectangle, ptList);
                            _currentScanInfo.ScanRegionList.Add(sr);
                            Well well = new Well(sr.WellId, sr.Bound);
                            _currentScanInfo.ScanWellList.Add(well);
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Sample")
                    break;
            }
        }

        private void LoadMicroplate(XmlReader reader)
        {
            int wellId = 0;
            int regionId = 0;
            int row = 0, column = 0;
            double wellWidth = 0.0, wellHeight = 0.0;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Wells":
                            row = XmlConvert.ToInt16(reader["startRow"]);
                            column = XmlConvert.ToInt16(reader["startRow"]);
                            int rows = XmlConvert.ToInt16(reader["rows"]);
                            int columns = XmlConvert.ToInt16(reader["columns"]);
                            wellId = row * columns + column * rows;
                            wellWidth = Math.Abs(XmlConvert.ToDouble(reader["wellOffsetXMM"]));
                            wellHeight = XmlConvert.ToDouble(reader["wellOffsetYMM"]);
                            break;
                        case "SubImages":

                            double transOffsetX = XmlConvert.ToDouble(reader["transOffsetXMM"]);
                            double transOffsetY = XmlConvert.ToDouble(reader["transOffsetYMM"]);

                            int subRows = XmlConvert.ToInt16(reader["subRows"]);
                            int subColumns = XmlConvert.ToInt16(reader["subColumns"]);

                            double subOffsetX = Math.Round(XmlConvert.ToDouble(reader["subOffsetXMM"]), 5);
                            double subOffsetY = Math.Round(XmlConvert.ToDouble(reader["subOffsetYMM"]), 5);

                            double hScanRegionWidth = subRows * subOffsetY * 500;
                            double hScanRegionHeight = subColumns * subOffsetX * 500;

                            double wellCenterX = (column * wellWidth - wellWidth / 2 + transOffsetX) * 1000;
                            double wellCenterY = (row * wellHeight - wellHeight / 2 + transOffsetY) * 1000;

                            Point[] ptList = new Point[4];
                            ptList[0].X = wellCenterX - hScanRegionWidth;
                            ptList[0].Y = wellCenterY - hScanRegionHeight;
                            ptList[1].X = wellCenterX + hScanRegionWidth;
                            ptList[1].Y = wellCenterY - hScanRegionHeight;
                            ptList[2].X = wellCenterX - hScanRegionWidth;
                            ptList[2].Y = wellCenterY + hScanRegionHeight;
                            ptList[3].X = wellCenterX + hScanRegionWidth;
                            ptList[3].Y = wellCenterY + hScanRegionHeight;

                            ScanRegion sr = new ScanRegion(wellId, regionId++, RegionShape.Rectangle, ptList);
                            _currentScanInfo.ScanRegionList.Add(sr);
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Sample")
                    break;
            }
        }

        private void SetCaptureMode(ScanInfo scanInfo)
        {
            CaptureMode captureMode;
            if (scanInfo.ThirdDimensionSteps > 1)
            {
                if (scanInfo.StreamFrameCount > 1 && scanInfo.TimingFrameCount > 1)
                {
                    captureMode = CaptureMode.Mode3DTimingStream;
                }
                else if (scanInfo.StreamFrameCount > 1)
                {
                    captureMode = CaptureMode.Mode3DStream;
                }
                else if (scanInfo.TimingFrameCount > 1)
                {
                    captureMode = CaptureMode.Mode3DTiming;
                }
                else
                {
                    captureMode = CaptureMode.Mode3D;
                }

                if (_captureMode == 1)
                {
                    captureMode = CaptureMode.Mode3DFastZStream;
                }
            }
            else
            {
                if (scanInfo.StreamFrameCount > 1 && scanInfo.TimingFrameCount > 1)
                {
                    captureMode = CaptureMode.Mode2DTimingStream;
                }
                else if (scanInfo.StreamFrameCount > 1)
                {
                    captureMode = CaptureMode.Mode2DStream;
                }
                else if (scanInfo.TimingFrameCount > 1)
                {
                    captureMode = CaptureMode.Mode2DTiming;
                }
                else
                {
                    captureMode = CaptureMode.Mode2D;
                }
            }

            scanInfo.Mode = captureMode;
        }

        private void SetFileMode(ScanInfo scanInfo)
        {
            var dir = new DirectoryInfo(scanInfo.DataPath);
            FileInfo[] files = dir.GetFiles("Image_0001_0001.raw");
            if (files.Length > 0)
            {
                scanInfo.ImageFormat = ImageFileFormat.Raw;
            }
            else
            {
                scanInfo.ImageFormat = ImageFileFormat.Tiff;
            }
        }
    }
}
