using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StackMonitor {
  public partial class MonitorReputationNotificator : Form {
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
    Reputation _reputation;
    Timer _closeTimer = new Timer();
    int _noofelements = 0;

    public MonitorReputationNotificator(Reputation reputation) {
      switch (reputation.Type) {
        case "answer":
          if (reputation.PositiveRep % 10 != 0) _noofelements++;
          if (reputation.PositiveRep > 0 && reputation.PositiveRep != 15) _noofelements++;
          if (reputation.NegativeRep > 0) _noofelements++;
          break;
        case "question":
          if (reputation.PositiveRep > 0) _noofelements++;
          if (reputation.NegativeRep > 0) _noofelements++;
          break;
      }
      InitializeComponent(70 + (30 * _noofelements));
      SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
      TransparencyKey = Color.HotPink;
      _reputation = reputation;
      GenerateFormImage(reputation);
      animator = new MonitorAnimator(this);
      animator.ScrollUp();
      if (_displayTime > 0)
      {
        _closeTimer.Interval = _displayTime;
        _closeTimer.Tick += _closeTimer_Tick;
        _closeTimer.Start();
      }
    }

    void _closeTimer_Tick(object sender, EventArgs e) {
      _closeTimer.Stop();
      animator.ScrollDown();
    }

    private void GenerateFormImage(Reputation reputation) {
      if (_formImage != null) {
        _formImage.Dispose();
      }
      _formImage = new Bitmap(400, this.Height);
      using (Graphics g = Graphics.FromImage(_formImage))
      using (GraphicsPath path = HelperClass.GenerateRoundedRectPath(new Rectangle(0, 0, 400, this.Height), 5, HelperClass.RectangleCorners.All))
      using (Brush backgroundbrush = new LinearGradientBrush(Point.Empty, new Point(0, this.Height), Color.FromArgb(212, 212, 212), Color.FromArgb(145, 145, 145)))
      using (SolidBrush darkGreyBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
      using (StringFormat centerformat = new StringFormat())
      using (Font arial10 = new Font("Arial", 10f))
      using (Font arial8 = new Font("Arial", 8f))
      using (Font arial8bold = new Font("Arial", 8f, FontStyle.Bold))
      using (Font arial12 = new Font("Arial", 12f))
      using (Font arial11 = new Font("Arial", 11f)) {
        centerformat.Alignment = StringAlignment.Near;
        centerformat.LineAlignment = StringAlignment.Center;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.Clear(Color.HotPink);
        g.FillPath(backgroundbrush, path);
        g.DrawPath(Pens.DarkGray, path);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.DrawString("REPUTATION CHANGE FOR " + reputation.Type.ToUpper(), arial8bold, Brushes.White, new Point(3, 3));
        g.DrawString("REPUTATION CHANGE FOR " + reputation.Type.ToUpper(), arial8bold, Brushes.Gray, new Point(2, 2));
        //draw closing x
        g.DrawLine(Pens.White, new Point(Bounds.Width - 14, 11), new Point(Bounds.Width - 4, 21));
        g.DrawLine(Pens.White, new Point(Bounds.Width - 14, 21), new Point(Bounds.Width - 4, 11));
        g.DrawLine(Pens.Black, new Point(Bounds.Width - 15, 10), new Point(Bounds.Width - 5, 20));
        g.DrawLine(Pens.Black, new Point(Bounds.Width - 15, 20), new Point(Bounds.Width - 5, 10));
        g.DrawString(reputation.Title, arial11, Brushes.White, new Rectangle(11, 16, 380, 35));
        g.DrawString(reputation.Title, arial11, Brushes.Black, new Rectangle(10, 15, 380, 35));
        int yposition = 60;
        //first, see if the reputation change contains a accepted answer ( +15 rep )
        if (reputation.Type == "answer" && reputation.PositiveRep % 10 != 0) {
          g.DrawImage(Properties.Resources.img_acceptedanswer, new Rectangle(30, yposition + 3, 24, 24));
          g.DrawString("Your answer is marked as accepted!", arial10, Brushes.White, new Rectangle(61, yposition + 1, 330, 30), centerformat);
          g.DrawString("Your answer is marked as accepted!", arial10, Brushes.Black, new Rectangle(60, yposition, 330, 30), centerformat);
          reputation.PositiveRep -= 15;
          yposition += 30;
        }
        if (reputation.PositiveRep > 0) {
          int count = 0;
          if (reputation.Type == "answer")
            count = reputation.PositiveRep / 10;
          else
            count = reputation.PositiveRep / 5;
          g.DrawImage(Properties.Resources.img_upvote, new Rectangle(30, yposition + 3, 24, 24));
          g.DrawString(count + " new upvote" + (count > 1 ? "s" : "") + " received.", arial10, Brushes.White, new Rectangle(61, yposition + 1, 330, 30), centerformat);
          g.DrawString(count + " new upvote" + (count > 1 ? "s" : "") + " received.", arial10, Brushes.Black, new Rectangle(60, yposition, 330, 30), centerformat);
          yposition += 30;
        }
        if (reputation.NegativeRep > 0) {
          g.DrawImage(Properties.Resources.img_downvote, new Rectangle(30, yposition + 3, 24, 24));
          int count = reputation.NegativeRep / 2;
          g.DrawString(count + " new downvote" + (count > 1 ? "s" : "") + " received.", arial10, Brushes.White, new Rectangle(61, yposition + 1, 330, 30), centerformat);
          g.DrawString(count + " new downvote" + (count > 1 ? "s" : "") + " received.", arial10, Brushes.Black, new Rectangle(60, yposition, 330, 30), centerformat);
        }
      }
    }

    protected override void OnPaint(PaintEventArgs e) {
      e.Graphics.DrawImageUnscaled(_formImage, Point.Empty);
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnMouseClick(MouseEventArgs e) {
      if (new Rectangle(Width - 30, 0, 30, 30).Contains(e.Location)) {
        animator.ScrollDown();
      } else {
        if (_reputation.QuestionID > 0) {
          Process.Start(@"http://www.stackoverflow.com/questions/" + _reputation.QuestionID);
        }
        animator.FadeOut();
      }
    }

    protected override void OnClosing(CancelEventArgs e) {
      base.OnClosing(e);
      if (_formImage != null) {
        _formImage.Dispose();
      }
    }
  }
}
