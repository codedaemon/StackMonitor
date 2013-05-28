using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace StackMonitor {

  public partial class MonitorBadgeNotificator : Form {
    #region Interop
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    [Flags()]
    private enum SetWindowPosFlags : uint {
      SWP_NOSIZE = 0x0001,
      SWP_NOACTIVATE = 0x0010
    }
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
    #endregion

    private static int _displayTime = 0;
    public static int DisplayTime
    {
      get { return _displayTime; }
      set { _displayTime = value; }
    }

    Image _formImage;
    MonitorAnimator animator;
    Timer _closeTimer = new Timer();

    public MonitorBadgeNotificator(Badge badge) {
      InitializeComponent();
      SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
      TransparencyKey = Color.HotPink;
      GenerateFormImage(badge);
      animator = new MonitorAnimator(this);
      animator.ScrollUp();
      if (_displayTime > 0)
      {
        _closeTimer.Interval = _displayTime;
        _closeTimer.Tick += new EventHandler(_closeTimer_Tick);
        _closeTimer.Start();
      }
    }

    void _closeTimer_Tick(object sender, EventArgs e) {
      _closeTimer.Stop();
      animator.ScrollDown();
    }

    private void GenerateFormImage(Badge badge) {
      if (_formImage != null) {
        _formImage.Dispose();
      }
      _formImage = new Bitmap(400, 90);
      using (Graphics g = Graphics.FromImage(_formImage))
      using (GraphicsPath path = HelperClass.GenerateRoundedRectPath(new Rectangle(0, 0, 400, 90), 5, HelperClass.RectangleCorners.All))
      using (GraphicsPath userPath = HelperClass.GenerateRoundedRectPath(new Rectangle(0, 101, 400, 48), 5, HelperClass.RectangleCorners.All))
      using (SolidBrush userPathBrush = new SolidBrush(Color.FromArgb(150, 255, 255, 255)))
      using (Brush backgroundbrush = new LinearGradientBrush(Point.Empty, new Point(0, 150), Color.FromArgb(212, 212, 212), Color.FromArgb(145, 145, 145)))
      using (SolidBrush darkGreyBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
      using (StringFormat centerformat = new StringFormat())
      using (Font arial10 = new Font("Arial", 10f))
      using (Font arial14 = new Font("Arial", 14f))
      using (Font arial8bold = new Font("Arial", 8f, FontStyle.Bold))
      using (Font arial12 = new Font("Arial", 12f))
      using (Font arial11 = new Font("Arial", 11f)) {
        centerformat.Alignment = StringAlignment.Center;
        centerformat.LineAlignment = StringAlignment.Center;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.Clear(Color.HotPink);
        g.FillPath(backgroundbrush, path);
        g.DrawPath(Pens.DarkGray, path);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawString("NEW BADGE AWARDED", arial8bold, Brushes.White, new Point(3, 3));
        g.DrawString("NEW BADGE AWARDED", arial8bold, Brushes.Gray, new Point(2, 2));
        //draw closing x
        g.DrawLine(Pens.White, new Point(Bounds.Width - 14, 11), new Point(Bounds.Width - 4, 21));
        g.DrawLine(Pens.White, new Point(Bounds.Width - 14, 21), new Point(Bounds.Width - 4, 11));
        g.DrawLine(Pens.Black, new Point(Bounds.Width - 15, 10), new Point(Bounds.Width - 5, 20));
        g.DrawLine(Pens.Black, new Point(Bounds.Width - 15, 20), new Point(Bounds.Width - 5, 10));
        g.DrawString(badge.Name, arial14, Brushes.White, new Rectangle(11, 21, 380, 35));
        g.DrawString(badge.Name, arial14, Brushes.Black, new Rectangle(10, 20, 380, 35));
        g.DrawString(badge.Description, arial12, Brushes.White, new Rectangle(51, 46, 340, 64));
        g.DrawString(badge.Description, arial12, Brushes.Black, new Rectangle(50, 45, 340, 64));
        g.FillEllipse(StackMonitorClient.BadgeBrushes[badge.Rank], new Rectangle(18, 50, 25, 25));
        using (GraphicsPath highlightpath = new GraphicsPath())
        using (SolidBrush highlightbrush = new SolidBrush(Color.FromArgb(50, 255, 255, 255))) {
          highlightpath.AddEllipse(new Rectangle(18, 50, 25, 25));
          g.SetClip(highlightpath);
          g.FillEllipse(highlightbrush, new Rectangle(13, 61, 45, 30));
        }
        g.ResetClip();
        g.DrawEllipse(Pens.Black, new Rectangle(18, 50, 25, 25));
      }
    }

    protected override void OnPaint(PaintEventArgs e) {
      e.Graphics.DrawImageUnscaled(_formImage, Point.Empty);
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnMouseClick(MouseEventArgs e) {
      if (new Rectangle(Width - 30, 0, 30, 30).Contains(e.Location)) {
        animator.ScrollDown();
      }
    }
  }
}
