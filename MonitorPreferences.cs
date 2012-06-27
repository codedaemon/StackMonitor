using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace StackMonitor
{
  public partial class MonitorPreferences : Form
  {

    LinearGradientBrush backgroundBrush = null;
    StackOverflowDataManager dm = null;
    StackMonitorClient mainApplication = null;
    int _hoveredTagGroup = -1;

    public MonitorPreferences(StackMonitorClient clientform)
    {
      InitializeComponent();
      tabPageTagList.MouseMove += TagMouseMove;
      tabPageTagList.MouseUp += TagMouseUp;
      typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(tabPageTagList, true, null);
      mainApplication = clientform;
      backgroundBrush = new LinearGradientBrush(Point.Empty, new Point(0, this.Bounds.Height), Color.LightGray, Color.Gray);
      textBoxNewQuestionTimeout.Text = (MonitorQuestionNotificator.DisplayTime / 1000).ToString();
      textBoxReputationChanges.Text = (MonitorReputationNotificator.DisplayTime / 1000).ToString();
      textBoxNewBadges.Text = (MonitorBadgeNotificator.DisplayTime / 1000).ToString();
      dm = new StackOverflowDataManager();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      //e.Graphics.FillRectangle(backgroundBrush, new Rectangle(Point.Empty, this.Bounds.Size));
      //DrawUserInformationBox(e.Graphics);
    }

    private void DrawTagGroupInfo(Graphics g)
    {
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
      g.SmoothingMode = SmoothingMode.HighQuality;
      DrawTagGroups(g);
    }

    private void DrawConnectedUserInfo(Graphics g)
    {
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
      g.SmoothingMode = SmoothingMode.HighQuality;
      DrawUserBox(g);
    }

    private void DrawTagGroups(Graphics g)
    {
      Rectangle tagGroupRect = new Rectangle((tabPageConnUser.Width - 400) / 2, buttonConnectUser.Bounds.Bottom + 40, 400, 300);//new Rectangle(buttonRegisterTagGroup.Bounds.X, buttonRegisterTagGroup.Bounds.Bottom + 5, buttonRegisterTagGroup.Bounds.Width, 300);
      using (GraphicsPath taggrouppath = HelperClass.GenerateRoundedRectPath(tagGroupRect, 5, HelperClass.RectangleCorners.All))
      using (LinearGradientBrush backbrush = new LinearGradientBrush(tagGroupRect.Location, new Point(tagGroupRect.X, tagGroupRect.Bottom), Color.FromArgb(240, 240, 240), Color.FromArgb(210, 210, 210)))
      using (Font tagfont = new Font("Arial", 10f))
      using (StringFormat format = new StringFormat())
      {
        format.Alignment = StringAlignment.Near; format.LineAlignment = StringAlignment.Center;
        g.FillPath(backbrush, taggrouppath);
        g.DrawPath(Pens.White, taggrouppath);
        int yposition = tagGroupRect.Y + 10;
        for (int i = 0; i < mainApplication.TagGroupList.Count; i++)
        {
          string taggroup = mainApplication.TagGroupList[i];
          g.DrawString(taggroup.Replace(";", " | "), tagfont, Brushes.White, new Rectangle(tagGroupRect.X + 11, yposition + 1, tagGroupRect.Width - 30, 30), format);
          g.DrawString(taggroup.Replace(";", " | "), tagfont, Brushes.Black, new Rectangle(tagGroupRect.X + 10, yposition, tagGroupRect.Width - 30, 30), format);
          if (_hoveredTagGroup == i)
          {
            g.DrawImage(Properties.Resources.img_delete_hover, new Rectangle(tagGroupRect.Right - 23, yposition + 5, Properties.Resources.img_delete_hover.Width, Properties.Resources.img_delete_hover.Height));
          }
          else
          {
            g.DrawImage(Properties.Resources.img_deletetag, new Rectangle(tagGroupRect.Right - 21, yposition + 7, Properties.Resources.img_deletetag.Width, Properties.Resources.img_deletetag.Height));
          }
          yposition += 30;
        }
      }
    }

    private void DrawUserBox(Graphics g)
    {
      Rectangle userboxRect = new Rectangle((tabPageConnUser.Width - 400) / 2, buttonConnectUser.Bounds.Bottom + 25, 400, 300);
      using (GraphicsPath connecteduserpath = HelperClass.GenerateRoundedRectPath(userboxRect, 5, HelperClass.RectangleCorners.All))
      using (LinearGradientBrush backbrush = new LinearGradientBrush(userboxRect.Location, new Point(userboxRect.X, userboxRect.Bottom), Color.FromArgb(240, 240, 240), Color.FromArgb(210, 210, 210)))
      using (Font mainfont = new Font("Arial", 14f))
      using (Font idfont = new Font("Arial", 12f))
      using (StringFormat format = new StringFormat())
      using (StringFormat centerformat = new StringFormat())
      {
        format.Alignment = StringAlignment.Far; format.LineAlignment = StringAlignment.Center;
        centerformat.Alignment = StringAlignment.Center; centerformat.LineAlignment = StringAlignment.Center;
        g.FillPath(backbrush, connecteduserpath);
        g.DrawPath(Pens.White, connecteduserpath);
        if (mainApplication.ConnectedUser != null)
        {
          using (Font repfont = new Font("Arial", 18f))
          {
            g.DrawString("ID: " + mainApplication.ConnectedUser.Id.ToString(), idfont, Brushes.White, new Rectangle(userboxRect.X + 1, userboxRect.Y + 4, userboxRect.Width - 3, 15), format);
            g.DrawString("ID: " + mainApplication.ConnectedUser.Id.ToString(), idfont, Brushes.Gray, new Rectangle(userboxRect.X, userboxRect.Y + 3, userboxRect.Width - 3, 15), format);
            format.Alignment = StringAlignment.Center;
            g.DrawString(mainApplication.ConnectedUser.Name, mainfont, Brushes.White, new Rectangle(userboxRect.X + 1, userboxRect.Y + 18, userboxRect.Width, 20), format);
            g.DrawString(mainApplication.ConnectedUser.Name, mainfont, Brushes.Black, new Rectangle(userboxRect.X, userboxRect.Y + 17, userboxRect.Width, 20), format);
            if (mainApplication.ConnectedUser.Image != null)
            {
              Point location = new Point(userboxRect.X + (userboxRect.Width - mainApplication.ConnectedUser.Image.Width) / 2, userboxRect.Y + 40);
              using (GraphicsPath path = HelperClass.GenerateRoundedRectPath(new Rectangle(location, new Size(128, 128)), 5, HelperClass.RectangleCorners.All))
              {
                g.SetClip(path);
                g.DrawImage(mainApplication.ConnectedUser.Image, location);
                g.ResetClip();
              }
            }
            g.DrawString(mainApplication.ConnectedUser.ReputationString, repfont, Brushes.White, new Rectangle(userboxRect.X + 1, userboxRect.Y + 171, userboxRect.Width, 30), format);
            g.DrawString(mainApplication.ConnectedUser.ReputationString, repfont, Brushes.DarkBlue, new Rectangle(userboxRect.X, userboxRect.Y + 170, userboxRect.Width, 30), format);
            for (int i = 0; i < mainApplication.ConnectedUser.Badges.Count; i++)
            {
              g.FillEllipse(StackMonitorClient.BadgeBrushes[mainApplication.ConnectedUser.Badges[i].Rank], new Rectangle(userboxRect.X + 50, userboxRect.Y + 210 + 30 * i, 20, 20));
              g.DrawEllipse(Pens.Black, new Rectangle(userboxRect.X + 50, userboxRect.Y + 210 + 30 * i, 20, 20));
              g.DrawString(mainApplication.ConnectedUser.Badges[i].Count.ToString(), mainfont, Brushes.White, new Rectangle(userboxRect.X + 71, userboxRect.Y + 211 + 30 * i, userboxRect.Width - 140, 20), format);
              g.DrawString(mainApplication.ConnectedUser.Badges[i].Count.ToString(), mainfont, Brushes.Black, new Rectangle(userboxRect.X + 70, userboxRect.Y + 210 + 30 * i, userboxRect.Width - 140, 20), format);
            }
          }
        }
        else
        {
          g.DrawString("Not connected to any user", mainfont, Brushes.White, new Rectangle(userboxRect.X + 1, userboxRect.Y + 1, userboxRect.Width, userboxRect.Height), centerformat);
          g.DrawString("Not connected to any user", mainfont, Brushes.Black, userboxRect, centerformat);
        }
      }
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    private void TagMouseMove(object sender, MouseEventArgs e)
    {
      Rectangle tagGroupRect = new Rectangle((tabPageConnUser.Width - 400) / 2, buttonConnectUser.Bounds.Bottom + 40, 400, 300);
      for (int i = 0; i < mainApplication.TagGroupList.Count(); i++)
      {
        if (
          new Rectangle(tagGroupRect.Right - 21, tagGroupRect.Y + 17 + (i * 30),
                        Properties.Resources.img_deletetag.Width, Properties.Resources.img_deletetag.Height).Contains( e.Location ))
        {
          _hoveredTagGroup = i;
          tabPageTagList.Invalidate();
          return;
        }
      }
      _hoveredTagGroup = -1;
      tabPageTagList.Invalidate();
    }

    private void TagMouseUp(object sender, MouseEventArgs e)
    {
      if (_hoveredTagGroup > -1)
      {
        mainApplication.TagGroupList.RemoveAt(_hoveredTagGroup);
        _hoveredTagGroup = -1;
        Invalidate();
      }
    }

    private void buttonConnectUser_Click(object sender, EventArgs e)
    {
      if (textBoxUserId.Text.Length == 0)
      {
        MessageBox.Show("Please enter your user ID in the text box above before clicking the button.");
        return;
      }
      int userid = 0;
      if (int.TryParse(textBoxUserId.Text, out userid))
      {
        User user = dm.PopulateUserInformation(userid, 128);
        if (user != null)
        {
          mainApplication.ConnectedUser = user;
          tabPageConnUser.Invalidate();
        }
        else
        {
          MessageBox.Show("User " + userid + " not found. Check that the entered User-ID is correct!");
        }
      }
      else
      {
        MessageBox.Show("Stack Overflow User-ID must be numeric");
      }
    }

    private void buttonRegisterTagGroup_Click(object sender, EventArgs e)
    {
      //TODO: verify that all the tags are existing before registering the tag group
      //exchange both , and <space> with ; to keep the list in a correct form
      var text = textBoxTagGroup.Text.Replace(' ', ';').Replace(',', ';').Replace('|', ';');
      mainApplication.TagGroupList.Add(text);
      tabPageTagList.Invalidate();
      textBoxTagGroup.Text = string.Empty;
    }

    private void MonitorPreferences_FormClosing(object sender, FormClosingEventArgs e)
    {
      int tempint = 0;
      if (int.TryParse(textBoxNewQuestionTimeout.Text, out tempint))
      {
        MonitorQuestionNotificator.DisplayTime = (tempint * 1000);
      }
      if (int.TryParse(textBoxReputationChanges.Text, out tempint))
      {
        MonitorReputationNotificator.DisplayTime = (tempint * 1000);
      }
      if (int.TryParse(textBoxNewBadges.Text, out tempint))
      {
        MonitorBadgeNotificator.DisplayTime = (tempint * 1000);
      }
    }

    private void tabPage1_Paint(object sender, PaintEventArgs e)
    {
      DrawConnectedUserInfo(e.Graphics);
    }

    private void tabPage2_Paint(object sender, PaintEventArgs e)
    {
      DrawTagGroupInfo(e.Graphics);
    }

  }
}
