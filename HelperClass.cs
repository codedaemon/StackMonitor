using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace StackMonitor {
  public static class HelperClass {
    public enum RectangleCorners {
      None = 0, TopLeft = 1, TopRight = 2, BottomLeft = 4, BottomRight = 8,
      All = TopLeft | TopRight | BottomLeft | BottomRight
    }

    public static GraphicsPath GenerateRoundedRectPath(Rectangle bounds, int roundradius, RectangleCorners roundedcorners) {
      GraphicsPath path = new GraphicsPath();
      int doubleround = roundradius * 2;
      path.AddLine(new Point(bounds.X + (((roundedcorners & RectangleCorners.TopLeft) > 0) ? roundradius : 0), bounds.Y), new Point(bounds.X + bounds.Width - (((roundedcorners & RectangleCorners.TopRight) > 0) ? roundradius : 0), bounds.Y));
      if ((roundedcorners & RectangleCorners.TopRight) > 0) {
        path.AddArc(new Rectangle(bounds.X + bounds.Width - doubleround, bounds.Y, doubleround, doubleround), 270, 90);
      }
      path.AddLine(new Point(bounds.X + bounds.Width, bounds.Y + (((roundedcorners & RectangleCorners.TopRight) > 0) ? roundradius : 0)), new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height - (((roundedcorners & RectangleCorners.BottomRight) > 0) ? roundradius : 0)));
      if ((roundedcorners & RectangleCorners.BottomRight) > 0) {
        path.AddArc(new Rectangle(bounds.X + bounds.Width - doubleround, bounds.Y + bounds.Height - doubleround, doubleround, doubleround), 0, 90);
      }
      path.AddLine(new Point(bounds.X + bounds.Width - (((roundedcorners & RectangleCorners.BottomRight) > 0) ? roundradius : 0), bounds.Y + bounds.Height), new Point(bounds.X + (((roundedcorners & RectangleCorners.BottomLeft) > 0) ? roundradius : 0), bounds.Y + bounds.Height));
      if ((roundedcorners & RectangleCorners.BottomLeft) > 0) {
        path.AddArc(new Rectangle(bounds.X, bounds.Y + bounds.Height - doubleround, doubleround, doubleround), 90, 90);
      }
      path.AddLine(new Point(bounds.X, bounds.Y + bounds.Height - (((roundedcorners & RectangleCorners.BottomLeft) > 0) ? roundradius : 0)), new Point(bounds.X, bounds.Y + (((roundedcorners & RectangleCorners.TopLeft) > 0) ? roundradius : 0)));
      if ((roundedcorners & RectangleCorners.TopLeft) > 0) {
        path.AddArc(new Rectangle(bounds.X, bounds.Y, doubleround, doubleround), 180, 90);
      }
      return path;
    }
  }
}
