using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace StackMonitor
{

  public partial class MonitorQuestionNotificator : Form
  {
    #region Interop
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    [Flags()]
    private enum SetWindowPosFlags : uint
    {
      SWP_NOSIZE = 0x0001,
      SWP_NOACTIVATE = 0x0010
    }



    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
    #endregion

    private static int _displayTime = 30000;
    public static int DisplayTime
    {
      get { return _displayTime; }
      set { _displayTime = value; }
    }

    Image _formImage;
    MonitorAnimator animator;
    Question _question;
    System.Windows.Forms.Timer _closeTimer = new System.Windows.Forms.Timer();

    public MonitorQuestionNotificator(Question question)
    {
      InitializeComponent();
      SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
      this.TransparencyKey = Color.HotPink;
      _question = question;
      GenerateFormImage(question);
      animator = new MonitorAnimator(this);
      animator.ScrollUp();
      if (_displayTime > 0)
      {
        _closeTimer.Interval = _displayTime;
        _closeTimer.Tick += new EventHandler(_closeTimer_Tick);
        _closeTimer.Start();
      }
    }

    void _closeTimer_Tick(object sender, EventArgs e)
    {
      _closeTimer.Stop();
      animator.ScrollDown();
    }

    private void GenerateFormImage(Question question)
    {
      if (_formImage != null)
      {
        _formImage.Dispose();
      }
      _formImage = new Bitmap(400, 200);
      using (Graphics g = Graphics.FromImage(_formImage))
      using (GraphicsPath path = HelperClass.GenerateRoundedRectPath(new Rectangle(0, 0, 400, 200), 5, HelperClass.RectangleCorners.All))
      using (GraphicsPath userPath = HelperClass.GenerateRoundedRectPath(new Rectangle(0, 136, 400, 48), 5, HelperClass.RectangleCorners.BottomLeft | HelperClass.RectangleCorners.BottomRight))
      using (SolidBrush userPathBrush = new SolidBrush(Color.FromArgb(150, 255, 255, 255)))
      using (Brush backgroundbrush = new LinearGradientBrush(Point.Empty, new Point(0, 185), Color.FromArgb(212, 212, 212), Color.FromArgb(145, 145, 145)))
      using (SolidBrush darkGreyBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
      using (StringFormat centerformat = new StringFormat())
      using (Font arial10 = new Font("Arial", 10f))
      using (Font arial8 = new Font("Arial", 8f))
      using (Font arial8bold = new Font("Arial", 8f, FontStyle.Bold))
      using (Font arial12 = new Font("Arial", 12f))
      using (Font arial11 = new Font("Arial", 11f))
      {
        centerformat.Alignment = StringAlignment.Center;
        centerformat.LineAlignment = StringAlignment.Center;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.Clear(Color.HotPink);
        g.FillPath(backgroundbrush, path);
        g.DrawPath(Pens.DarkGray, path);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.DrawString("NEW QUESTION", arial8bold, Brushes.White, new Point(3, 3));
        g.DrawString("NEW QUESTION", arial8bold, Brushes.Gray, new Point(2, 2));
        //draw closing x
        g.DrawLine(Pens.White, new Point(Bounds.Width - 14, 11), new Point(Bounds.Width - 4, 21));
        g.DrawLine(Pens.White, new Point(Bounds.Width - 14, 21), new Point(Bounds.Width - 4, 11));
        g.DrawLine(Pens.Black, new Point(Bounds.Width - 15, 10), new Point(Bounds.Width - 5, 20));
        g.DrawLine(Pens.Black, new Point(Bounds.Width - 15, 20), new Point(Bounds.Width - 5, 10));
        g.DrawString(question.Title, arial11, Brushes.White, new Rectangle(11, 13, 380, 45));
        g.DrawString(question.Title, arial11, Brushes.Black, new Rectangle(10, 12, 380, 45));
        g.DrawString(question.Body, arial8, Brushes.White, new Rectangle(11, 56, 380, 60));
        g.DrawString(question.Body, arial8, Brushes.Black, new Rectangle(10, 55, 380, 60));
        string tagstring = string.Empty;
        question.Tags.ForEach(t => tagstring += t + " | ");
        tagstring = tagstring.Remove(tagstring.Length - 3);
        g.DrawLine(Pens.White, new Point(11, 119), new Point(Width - 9, 119));
        g.DrawLine(Pens.Black, new Point(10, 118), new Point(Width - 10, 118));
        g.DrawString(tagstring, arial10, Brushes.White, new PointF(11, 120));
        g.DrawString(tagstring, arial10, Brushes.Black, new PointF(10, 119));
        g.FillPath(userPathBrush, userPath);
        g.DrawPath(Pens.Gray, userPath);
        g.DrawString(question.Owner.Name, arial12, Brushes.DarkBlue, new Rectangle(50, 138, 345, 20));
        g.DrawString(question.Owner.ReputationString, arial12, Brushes.Black, new Rectangle(50, 162, 80, 20), centerformat);
        int xposition = 135;
        foreach (BadgeGroup badge in question.Owner.Badges.Where(b => b.Count > 0))
        {
          g.FillEllipse(StackMonitorClient.BadgeBrushes[badge.Rank], new Rectangle(xposition, 165, 10, 10));
          g.DrawEllipse(Pens.Black, new Rectangle(xposition, 165, 10, 10));
          g.DrawString(badge.Count.ToString(), arial12, Brushes.Black, new Rectangle(xposition + 10, 162, 35, 20), centerformat);
          xposition += 40;
        }
        if (question.Owner.Image != null) g.DrawImage(question.Owner.Image, new Rectangle(10, 144, 32, 32));
      }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      e.Graphics.DrawImageUnscaled(_formImage, Point.Empty);
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnMouseClick(MouseEventArgs e)
    {
      if (new Rectangle(Width - 30, 0, 30, 30).Contains(e.Location))
      {
        animator.ScrollDown();
      }
      else
      {
        if (!string.IsNullOrEmpty(_question.URL))
        {
          Process.Start(_question.URL);
        }
        animator.FadeOut();
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);
      StackMonitorClient.OpenQuestions.RemoveAll(q => q.Id == _question.Id);  
      if (_formImage != null)
      {
        _formImage.Dispose();
      }
    }
  }
}
