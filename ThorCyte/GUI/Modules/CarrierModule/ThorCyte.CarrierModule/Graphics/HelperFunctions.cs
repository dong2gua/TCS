using System.Collections.Generic;
using System.Windows.Input;
using ThorCyte.CarrierModule.Canvases;

namespace ThorCyte.CarrierModule.Graphics
{
    /// <summary>
    /// Helper class which contains general helper functions and properties.
    /// 
    /// Most functions in this class replace VisualCollection-derived class
    /// methods, because I cannot derive from VisualCollection.
    /// They make different operations with GraphicsBase list.
    /// </summary>
    static class HelperFunctions
    {
        /// <summary>
        /// Default cursor
        /// </summary>
        public static Cursor DefaultCursor
        {
            get
            {
                return Cursors.Arrow;
            }
        }

        public static Cursor CrossCursor
        {
            get { return Cursors.Cross; }
        }

        /// <summary>
        /// Select all graphic objects
        /// </summary>
        public static void SelectAll(SlideCanvas drawingCanvas)
        {
            for (var i = 0; i < drawingCanvas.Count; i++)
            {
                drawingCanvas[i].IsSelected = true;
            }
        }

        /// <summary>
        /// Unselect all graphic objects
        /// </summary>
        public static void UnselectAll(SlideCanvas drawingCanvas)
        {
            for (var i = 0; i < drawingCanvas.Count; i++)
            {
                drawingCanvas[i].IsSelected = false;
            }
        }

        /// <summary>
        /// Delete selected graphic objects
        /// </summary>
        public static void DeleteSelection(SlideCanvas drawingCanvas)
        {
            //CommandDelete command = new CommandDelete(drawingCanvas);
            var wasChange = false;

            for (var i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    drawingCanvas.GraphicsList.RemoveAt(i);
                    wasChange = true;
                }
            }

            if (wasChange)
            {
                //drawingCanvas.AddCommandToHistory(command);
            }
        }

        /// <summary>
        /// Delete all graphic objects
        /// </summary>
        public static void DeleteAll(SlideCanvas drawingCanvas)
        {
            if (drawingCanvas.GraphicsList.Count > 0)
            {
                //drawingCanvas.AddCommandToHistory(new CommandDeleteAll(drawingCanvas));

                drawingCanvas.GraphicsList.Clear();
            }

        }

        /// <summary>
        /// Move selection to front
        /// </summary>
        public static void MoveSelectionToFront(SlideCanvas drawingCanvas)
        {
            // Moving to front of z-order means moving
            // to the end of VisualCollection.

            // Read GraphicsList in the reverse order, and move every selected object
            // to temporary list.

            var list = new List<GraphicsBase>();

            //CommandChangeOrder command = new CommandChangeOrder(drawingCanvas);

            for (var i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    list.Insert(0, drawingCanvas[i]);
                    drawingCanvas.GraphicsList.RemoveAt(i);
                }
            }

            // Add all items from temporary list to the end of GraphicsList
            foreach (var g in list)
            {
                drawingCanvas.GraphicsList.Add(g);
            }

            if (list.Count > 0)
            {
                //command.NewState(drawingCanvas);
                //drawingCanvas.AddCommandToHistory(command);
            }
        }

        /// <summary>
        /// Move selection to back
        /// </summary>
        public static void MoveSelectionToBack(SlideCanvas drawingCanvas)
        {
            // Moving to back of z-order means moving
            // to the beginning of VisualCollection.

            // Read GraphicsList in the reverse order, and move every selected object
            // to temporary list.

            var list = new List<GraphicsBase>();

            //CommandChangeOrder command = new CommandChangeOrder(drawingCanvas);

            for (var i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    list.Add(drawingCanvas[i]);
                    drawingCanvas.GraphicsList.RemoveAt(i);
                }
            }

            // Add all items from temporary list to the beginning of GraphicsList
            foreach (var g in list)
            {
                drawingCanvas.GraphicsList.Insert(0, g);
            }

            if (list.Count > 0)
            {
                // command.NewState(drawingCanvas);
                // drawingCanvas.AddCommandToHistory(command);
            }
        }
    }


    static class PlateHelperFunctions
    {
        /// <summary>
        /// Default cursor
        /// </summary>
        public static Cursor DefaultCursor
        {
            get
            {
                return Cursors.Arrow;
            }
        }

        public static Cursor CrossCursor
        {
            get { return Cursors.Cross; }
        }

        /// <summary>
        /// Select all graphic objects
        /// </summary>
        public static void SelectAll(PlateCanvas drawingCanvas)
        {
            for (var i = 0; i < drawingCanvas.Count; i++)
            {
                drawingCanvas[i].IsSelected = true;
            }
        }

        /// <summary>
        /// Unselect all graphic objects
        /// </summary>
        public static void UnselectAll(PlateCanvas drawingCanvas)
        {
            for (var i = 0; i < drawingCanvas.Count; i++)
            {
                drawingCanvas[i].IsSelected = false;
            }
        }

        /// <summary>
        /// Delete selected graphic objects
        /// </summary>
        public static void DeleteSelection(PlateCanvas drawingCanvas)
        {
            //CommandDelete command = new CommandDelete(drawingCanvas);
            var wasChange = false;

            for (var i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    drawingCanvas.GraphicsList.RemoveAt(i);
                    wasChange = true;
                }
            }

            if (wasChange)
            {
                //drawingCanvas.AddCommandToHistory(command);
            }
        }

        /// <summary>
        /// Delete all graphic objects
        /// </summary>
        public static void DeleteAll(PlateCanvas drawingCanvas)
        {
            if (drawingCanvas.GraphicsList.Count > 0)
            {
                //drawingCanvas.AddCommandToHistory(new CommandDeleteAll(drawingCanvas));

                drawingCanvas.GraphicsList.Clear();
            }

        }

        /// <summary>
        /// Move selection to front
        /// </summary>
        public static void MoveSelectionToFront(PlateCanvas drawingCanvas)
        {
            // Moving to front of z-order means moving
            // to the end of VisualCollection.

            // Read GraphicsList in the reverse order, and move every selected object
            // to temporary list.

            var list = new List<GraphicsBase>();

            //CommandChangeOrder command = new CommandChangeOrder(drawingCanvas);

            for (var i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    list.Insert(0, drawingCanvas[i]);
                    drawingCanvas.GraphicsList.RemoveAt(i);
                }
            }

            // Add all items from temporary list to the end of GraphicsList
            foreach (var g in list)
            {
                drawingCanvas.GraphicsList.Add(g);
            }

            if (list.Count > 0)
            {
                //command.NewState(drawingCanvas);
                //drawingCanvas.AddCommandToHistory(command);
            }
        }

        /// <summary>
        /// Move selection to back
        /// </summary>
        public static void MoveSelectionToBack(PlateCanvas drawingCanvas)
        {
            // Moving to back of z-order means moving
            // to the beginning of VisualCollection.

            // Read GraphicsList in the reverse order, and move every selected object
            // to temporary list.

            var list = new List<GraphicsBase>();

            //CommandChangeOrder command = new CommandChangeOrder(drawingCanvas);

            for (var i = drawingCanvas.Count - 1; i >= 0; i--)
            {
                if (drawingCanvas[i].IsSelected)
                {
                    list.Add(drawingCanvas[i]);
                    drawingCanvas.GraphicsList.RemoveAt(i);
                }
            }

            // Add all items from temporary list to the beginning of GraphicsList
            foreach (var g in list)
            {
                drawingCanvas.GraphicsList.Insert(0, g);
            }

            if (list.Count > 0)
            {
                // command.NewState(drawingCanvas);
                // drawingCanvas.AddCommandToHistory(command);
            }
        }
    }


}
