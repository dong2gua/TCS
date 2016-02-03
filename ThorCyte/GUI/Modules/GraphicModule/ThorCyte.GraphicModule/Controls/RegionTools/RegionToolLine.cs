using System.Windows.Input;
using ThorCyte.GraphicModule.Controls.Graphics;

namespace ThorCyte.GraphicModule.Controls.RegionTools
{
    /// <summary>
    /// Line tool
    /// </summary>
    public class RegionToolLine : RegionToolObject
    {
        #region Properties

        public GraphicsLine HorizonLine { get; set; }

        public GraphicsLine VerticalLine { get; set; }

        #endregion

        #region Constructor

        public RegionToolLine(GraphicsLine horline, GraphicsLine verline, RegionCanvas graph)
        {
            HorizonLine = horline;
            VerticalLine = verline;
            AddNewObject(graph, HorizonLine);
            HorizonLine.MoveHandleTo(horline.End, 0);
            AddNewObject(graph, VerticalLine);
            VerticalLine.MoveHandleTo(verline.End, 0);
        }

        #endregion

        #region Methods
        /// <summary>
        /// Create new object
        /// </summary>
        public override void OnMouseDown(RegionCanvas graph, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Set cursor and resize new object.
        /// </summary>
        public override void OnMouseMove(RegionCanvas graph, MouseEventArgs e)
        {
        }

        #endregion
    }
}
