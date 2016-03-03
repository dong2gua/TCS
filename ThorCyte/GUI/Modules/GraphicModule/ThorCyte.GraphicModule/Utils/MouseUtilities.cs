using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace ThorCyte.GraphicModule.Utils
{
	/// <summary>
	/// Provides access to the mouse location by calling unmanaged code.
	/// </summary>
	/// <remarks>
	/// This class was written by Dan Crevier (Microsoft).  
	/// http://blogs.msdn.com/llobo/archive/2006/09/06/Scrolling-Scrollviewer-on-Mouse-Drag-at-the-boundaries.aspx
	/// </remarks>
	public class MouseUtilities
	{
		[StructLayout( LayoutKind.Sequential )]
		private struct Win32Point
		{
			public Int32 X;
			public Int32 Y;
		};

		[DllImport( "user32.dll" )]
		private static extern bool GetCursorPos( ref Win32Point pt );

		/// <summary>
		/// Returns the mouse cursor location.  This method is necessary during 
		/// a drag-drop operation because the WPF mechanisms for retrieving the
		/// cursor coordinates are unreliable.
		/// </summary>
		/// <param name="relativeTo">The Visual to which the mouse coordinates will be relative.</param>
		public static Point GetMousePosition( Visual relativeTo )
		{
			var mouse = new Win32Point();
			GetCursorPos( ref mouse );
			return relativeTo.PointFromScreen( new Point( mouse.X, mouse.Y ) );
		}
	}
}