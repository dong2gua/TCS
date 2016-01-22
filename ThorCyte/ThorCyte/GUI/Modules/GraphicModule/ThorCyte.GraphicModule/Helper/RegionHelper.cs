using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using ROIService;
using ROIService.Region;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.ViewModels;

namespace ThorCyte.GraphicModule.Helper
{
    public static class RegionHelper
    {
        public static void UpdateRegionLocation(MaskRegion region, GraphicsBase graphic, RegionCanvas canvas, GraphicVmBase vm)
        {
            double x, y, width, height;
            var xLogscale = (vm.XAxis.MaxRange - vm.XAxis.MinRange) / canvas.ActualWidth;
            var yLogscale = (vm.YAxis.MaxRange - vm.YAxis.MinRange) / canvas.ActualHeight;
            var rectRegion = region as RectangleRegion;

            if (rectRegion != null)
            {
                var rectGraphic = (GraphicsRectangleBase)graphic;
                if (vm.XAxis.IsLogScale)
                {
                    x = rectGraphic.Rectangle.Left * xLogscale;
                    width = rectGraphic.Rectangle.Right * xLogscale - x;
                }
                else
                {
                    x = rectGraphic.Rectangle.Left / canvas.XScale + vm.XAxis.MinRange; 
                    width = rectGraphic.Rectangle.Width / canvas.XScale;
                }

                if (vm.YAxis.IsLogScale)
                {
                    y = (canvas.ActualHeight - rectGraphic.Rectangle.Top) * yLogscale;
                    height = rectGraphic.Rectangle.Height * yLogscale;
                }
                else
                {
                    y = (canvas.ActualHeight - rectGraphic.Rectangle.Top) / canvas.YScale + vm.YAxis.MinRange;
                    height = rectGraphic.Rectangle.Height / canvas.YScale;
                }
                rectRegion.LeftUp = new Point(x, y);
                rectRegion.Size = new Size(width, height);
                return;
            }

            var polygonRegion = region as PolygonRegion;
            if (polygonRegion != null)
            {
                var polygonGraphic = (GraphicsPolygon)graphic;
                polygonRegion.Vertex.Clear();
                for (var index = 0; index < polygonGraphic.Points.Length; index++)
                {
                    var p = polygonGraphic.Points[index];
                    x = vm.XAxis.IsLogScale ? p.X * xLogscale : p.X / canvas.XScale + vm.XAxis.MinRange; ;
                    y = vm.YAxis.IsLogScale ? (canvas.ActualHeight - p.Y) * yLogscale : (canvas.ActualHeight - p.Y) / canvas.YScale + vm.YAxis.MinRange;
                    polygonRegion.Vertex.Add(new Point(x, y));
                }
                return;
            }

            var ellipseRegion = region as EllipseRegion;
            if (ellipseRegion != null)
            {
                var ellipseGraphic = (GraphicsEllipse)graphic;
                var centerx = ellipseGraphic.Rectangle.Left + ellipseGraphic.Rectangle.Width / 2.0;
                var centery = canvas.ActualHeight - (ellipseGraphic.Rectangle.Top + ellipseGraphic.Rectangle.Height / 2.0);

                if (vm.XAxis.IsLogScale)
                {
                    x = centerx * xLogscale;
                    width = ellipseGraphic.Rectangle.Width * xLogscale;
                }
                else
                {
                    x = centerx / canvas.XScale + vm.XAxis.MinRange;
                    width = ellipseGraphic.Rectangle.Width / canvas.XScale ;
                }

                if (vm.YAxis.IsLogScale)
                {
                    y = centery * yLogscale;
                    height = ellipseGraphic.Rectangle.Height * yLogscale;
                }
                else
                {
                    y = centery / canvas.YScale + vm.YAxis.MinRange ;
                    height = ellipseGraphic.Rectangle.Height / canvas.YScale;
                }

                ellipseRegion.Center = new Point(x, y);
                ellipseRegion.Axis = new Size(width, height);
            }
        }

        public static void SetCommonRegionParas(MaskRegion region, GraphicVmBase vm)
        {
            region.IsLogscaleX = vm.XAxis.IsLogScale;
            if (vm.XAxis.SelectedNumeratorFeature != null)
            {
                region.FeatureTypeNumeratorX = vm.XAxis.SelectedNumeratorFeature.FeatureType;
            }
            if (vm.XAxis.SelectedDenominatorFeature != null)
            {
                region.FeatureTypeDenominatorX = vm.XAxis.SelectedDenominatorFeature.FeatureType;
            }
            if (vm.XAxis.SelectedDenominatorChannel != null)
            {
                region.ChannelDenominatorX = vm.XAxis.SelectedDenominatorChannel.ChannelName;
            }
            if (vm.XAxis.SelectedNumeratorChannel != null)
            {
                region.ChannelNumeratorX = vm.XAxis.SelectedNumeratorChannel.ChannelName;
            }
        }


        public static void Set2DCommonRegionParas(MaskRegion region, ScattergramVm vm)
        {
            SetCommonRegionParas(region, vm);
            region.IsLogScaleY = vm.YAxis.IsLogScale;
            if (vm.YAxis.SelectedNumeratorFeature != null)
            {
                region.FeatureTypeNumeratorY = vm.YAxis.SelectedNumeratorFeature.FeatureType;
            }
            if (vm.YAxis.SelectedDenominatorFeature != null)
            {
                region.FeatureTypeDenominatorY = vm.YAxis.SelectedDenominatorFeature.FeatureType;
            }
            if (vm.SelectedZScaleFeature != null)
            {
                region.FeatureTypeZ = vm.SelectedZScaleFeature.FeatureType;
            }
            if (vm.YAxis.SelectedDenominatorChannel != null)
            {
                region.ChannelDenominatorY = vm.YAxis.SelectedDenominatorChannel.ChannelName;
            }
            if (vm.YAxis.SelectedNumeratorChannel != null)
            {
                region.ChannelNumeratorY = vm.YAxis.SelectedNumeratorChannel.ChannelName;
            }
            if (vm.SelecedZScaleChannel != null)
            {
                region.ChannelZ = vm.SelecedZScaleChannel.ChannelName;
            }
        }


        public static List<string> GetDescendants(List<MaskRegion> regionList, IList<MaskRegion> selfRegionList)
        {
            var descendantList = selfRegionList.Select(region =>ConstantHelper.PrefixRegionName + region.Id).ToList();
            var queue = selfRegionList.Select(region => region.Id.ToString(CultureInfo.InvariantCulture)).ToList();

            while (queue.Count > 0)
            {
                var first = queue.First();
                var tmpregion = regionList.FirstOrDefault(maskregion => maskregion.Id.ToString(CultureInfo.InvariantCulture) == first);
                if (tmpregion != null && tmpregion.Children != null)
                {
                    queue.AddRange(tmpregion.Children);
                    descendantList.AddRange(tmpregion.Children);
                }
                queue.RemoveAt(0);
            }
            return descendantList;
        }

        public static List<MaskRegion> GetSelfRegionList(IEnumerable<MaskRegion> regionList, string graphicId)
        {
            return regionList.Where(region => region.GraphicId == graphicId).ToList();
        }

        public static List<MaskRegion> GetRegionList()
        {
            var regionList = new List<MaskRegion>();
            var ids = ROIManager.Instance.GetRegionIdList();
            foreach (var id in ids)
            {
                var region = ROIManager.Instance.GetRegion(id);
                if (region != null)
                {
                    regionList.Add(region);
                }
            }
            return regionList;
        }

        public static void InitRegionRelationship()
        {
            var updateRegionList = new List<MaskRegion>();
            var regionList = GetRegionList();
            var graphicVmList = GraphicModule.GraphicManagerVmInstance.GetGraphicVmList();
            foreach (var regionItem in regionList)
            {
                var graphicVm = graphicVmList.Find(vm => vm.Id == regionItem.GraphicId);
                    if (graphicVm != null)
                    {
                        var left = (!string.IsNullOrEmpty(graphicVm.SelectedGate1) && graphicVm.SelectedGate1.StartsWith(ConstantHelper.PrefixRegionName)) ? graphicVm.SelectedGate1 : string.Empty;
                        var right = (!string.IsNullOrEmpty(graphicVm.SelectedGate2) && graphicVm.SelectedGate2.StartsWith(ConstantHelper.PrefixRegionName)) ? graphicVm.SelectedGate2 : string.Empty;
                        regionItem.LeftParent = left;
                        if (graphicVm.IsGate2Enable)
                        {
                            regionItem.RightParent = right;
                        }
                        if (graphicVm.IsOperatorEnable)
                        {
                            regionItem.Operation = graphicVm.SelectedOperator;
                        }
                        updateRegionList.Add(regionItem);
                    }
            }

            foreach (var regionItem in regionList)
            {
                var graphicVm = graphicVmList.Find(vm => vm.Id == regionItem.GraphicId);
                    if (graphicVm != null)
                    {
                        var left = (!string.IsNullOrEmpty(graphicVm.SelectedGate1) && graphicVm.SelectedGate1.StartsWith(ConstantHelper.PrefixRegionName)) ? graphicVm.SelectedGate1 : string.Empty;
                        var right = (!string.IsNullOrEmpty(graphicVm.SelectedGate2) && graphicVm.SelectedGate2.StartsWith(ConstantHelper.PrefixRegionName)) ? graphicVm.SelectedGate2 : string.Empty;
                        if (!string.IsNullOrEmpty(left))
                        {
                            var leftId = int.Parse(left.Remove(0,1));
                            var item = updateRegionList.Find(region => region.Id == leftId);
                            if (item != null)
                            {
                                regionItem.Children.Add(ConstantHelper.PrefixRegionName + regionItem.Id);
                            }
                        }
                        if (!string.IsNullOrEmpty(right))
                        {
                            var rightId = int.Parse(right.Remove(0, 1));
                            var item = updateRegionList.Find(regionTuple => regionTuple.Id == rightId);
                            if (item != null)
                            {
                                regionItem.Children.Add(ConstantHelper.PrefixRegionName + regionItem.Id);
                            }
                        }
                    }          
            }

            if (updateRegionList.Count > 0)
            {
                ROIManager.Instance.InitRegions(updateRegionList);
            }
        }


        public static void UpdateRelationShip(string graphId)
        {
            if (string.IsNullOrEmpty(graphId))
            {
                return;
            }
            var graphicVmList = GraphicModule.GraphicManagerVmInstance.GetGraphicVmList();
            var graphicVm = graphicVmList.Find(graphic => graphic.Id == graphId);

            var list = new List<MaskRegion>();
            var regionList = GetRegionList();

            foreach (var regionItem in regionList)
            {
                var id = ConstantHelper.PrefixRegionName + regionItem.Id; 
                if (regionItem.GraphicId == graphId)
                {
                    if (graphicVm != null)
                    {
                        var leftParent = (!string.IsNullOrEmpty(graphicVm.SelectedGate1) && graphicVm.SelectedGate1.StartsWith(ConstantHelper.PrefixRegionName)) ? graphicVm.SelectedGate1 : string.Empty;
                        var rightParent = (graphicVm.IsGate2Enable && !string.IsNullOrEmpty(graphicVm.SelectedGate2) && graphicVm.SelectedOperator != OperationType.None && graphicVm.SelectedGate2.StartsWith(ConstantHelper.PrefixRegionName)) ? graphicVm.SelectedGate2 : string.Empty;
                        var op = graphicVm.SelectedOperator;

                        if (regionItem.LeftParent != leftParent || regionItem.RightParent != rightParent || regionItem.Operation != op)
                        {
                            if (!string.IsNullOrEmpty(regionItem.LeftParent))
                            {
                                var temp = list.Find(maskregion => maskregion.Id.ToString(CultureInfo.InvariantCulture) == regionItem.LeftParent.Remove(0));
                                var leftRegion = temp ?? ROIManager.Instance.GetRegion(regionItem.LeftParent);
                                if (leftRegion.Children != null && leftRegion.Children.Contains(id))
                                {
                                    leftRegion.Children.Remove(id);
                                }
                                if (temp == null)
                                {
                                    list.Add(leftRegion);
                                }
                            }
                            if (!string.IsNullOrEmpty(regionItem.RightParent))
                            {
                                var temp = list.Find(maskregion => maskregion.Id.ToString(CultureInfo.InvariantCulture) == regionItem.RightParent.Remove(0));
                                var rightRegion = temp ?? ROIManager.Instance.GetRegion(regionItem.RightParent);
                                if (rightRegion.Children != null && rightRegion.Children.Contains(id))
                                {
                                    rightRegion.Children.Remove(id);
                                }
                                if (temp == null)
                                {
                                    list.Add(rightRegion);
                                }
                            }
                            regionItem.Operation = graphicVm.SelectedOperator;
                            regionItem.LeftParent = leftParent;
                            regionItem.RightParent = rightParent;
                            list.Add(regionItem);
                        }

                        MaskRegion tmpRegion;
                        MaskRegion parentRegion;
                        if (!string.IsNullOrEmpty(graphicVm.SelectedGate1) && graphicVm.SelectedGate1.StartsWith(ConstantHelper.PrefixRegionName))
                        {
                            tmpRegion = list.Find(region => region.Id.ToString(CultureInfo.InvariantCulture) == graphicVm.SelectedGate1.Remove(0));
                            parentRegion = tmpRegion ?? ROIManager.Instance.GetRegion(graphicVm.SelectedGate1);
                            if (parentRegion != null)
                            {
                                if (!parentRegion.Children.Contains(id))
                                {
                                    parentRegion.Children.Add(id);
                                    if (tmpRegion == null)
                                    {
                                        list.Add(parentRegion);
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(graphicVm.SelectedGate2))
                        {
                            tmpRegion = list.Find(region => region.Id.ToString(CultureInfo.InvariantCulture) == graphicVm.SelectedGate2.Remove(0));
                            parentRegion = tmpRegion ?? ROIManager.Instance.GetRegion(graphicVm.SelectedGate2);

                            if (parentRegion != null)
                            {
                                if (!parentRegion.Children.Contains(id))
                                {
                                    if (graphicVm.IsGate2Enable)
                                    {
                                        parentRegion.Children.Add(id);
                                        if (tmpRegion == null)
                                        {
                                            list.Add(parentRegion);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!graphicVm.IsGate2Enable)
                                    {
                                        parentRegion.Children.Remove(id);
                                        if (tmpRegion == null)
                                        {
                                            list.Add(parentRegion);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                ROIManager.Instance.UpdateRegions(list);
            }
        }
    }
}
