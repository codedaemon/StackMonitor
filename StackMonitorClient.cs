using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using StackMonitor.Properties;

namespace StackMonitor
{

  public enum NotificationType { NewQuestion, ReputationChange, NewBadge, UpdateUser }

  public partial class StackMonitorClient : Form
  {

    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Dictionary<BadgeRank, Brush> BadgeBrushes { get; set; }

    private User _connectedUser;
    public User ConnectedUser
    {
      get { return _connectedUser; }
      set
      {
        _connectedUser = value;
        if (_connectedUser != null)
        {
          connectedUserMenuItem.Text = _connectedUser.Name;
          reputationMenuItem.Text = "Reputation: " + _connectedUser.Reputation; reputationMenuItem.Visible = true;
          badgesMenuItem.Text = "Badges: " + _connectedUser.Badges.Where(b => b.Rank == BadgeRank.Gold).Sum(b => b.Count)
                                     + "," + _connectedUser.Badges.Where(b => b.Rank == BadgeRank.Silver).Sum(b => b.Count)
                                     + "," + _connectedUser.Badges.Where(b => b.Rank == BadgeRank.Bronze).Sum(b => b.Count);
          badgesMenuItem.Visible = true;
        }
        else
        {
          connectedUserMenuItem.Text = "Not connected";
          reputationMenuItem.Visible = false;
          badgesMenuItem.Visible = false;
        }
      }
    }
    public List<string> TagGroupList { get; set; }

    static StackMonitorClient()
    {
      BadgeBrushes = new Dictionary<BadgeRank, Brush>
                       {
                         {BadgeRank.Bronze, new SolidBrush(Color.FromArgb(204, 153, 102))},
                         {BadgeRank.Silver, new SolidBrush(Color.FromArgb(197, 197, 197))},
                         {BadgeRank.Gold, new SolidBrush(Color.FromArgb(255, 204, 0))}
                       };
    }

    readonly BackgroundWorker _monitorWorker = new BackgroundWorker();
    readonly StackOverflowDataManager datamanager = new StackOverflowDataManager();

    //int highestquestionnumber = -1;
    long questionlastchecktime = GetUnixTime(DateTime.UtcNow.AddMinutes(-5));
    long reputationlastchecktime = GetUnixTime(DateTime.UtcNow.AddDays(-3));
    long newbadgeschecktime = GetUnixTime(DateTime.UtcNow.AddDays(-5));

    public StackMonitorClient()
    {
      TagGroupList = new List<string>();
      MonitorQuestionNotificator.DisplayTime = Settings.Default.NewQuestionTimeout;
      MonitorReputationNotificator.DisplayTime = Settings.Default.ReputationChangeTimeout;
      MonitorBadgeNotificator.DisplayTime = Settings.Default.NewBadgeTimeout;
      if (Settings.Default.LastReputationCheckTime > -1) reputationlastchecktime = Settings.Default.LastReputationCheckTime;
      if (Settings.Default.LastBadgesCheckTime > -1) newbadgeschecktime = Settings.Default.LastBadgesCheckTime;
      InitializeComponent();
      _monitorWorker.DoWork += _monitorWorker_DoWork;
      _monitorWorker.WorkerSupportsCancellation = true;
      _monitorWorker.WorkerReportsProgress = true;
      _monitorWorker.ProgressChanged += _monitorWorker_ProgressChanged;
      _monitorWorker.RunWorkerAsync();
      if (Settings.Default.ConnectedUser > -1)
      {
        ConnectedUser = datamanager.PopulateUserInformation(Settings.Default.ConnectedUser, 128);
      }
      string[] splittaggroup = Settings.Default.TagGroupList.Split(new[] { '¤' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string taggroup in splittaggroup)
      {
        TagGroupList.Add(taggroup);
      }
    }

    public static long GetUnixTime(DateTime time)
    {
      return (long)(time - Epoch).TotalSeconds;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      _monitorWorker.CancelAsync();
    }

    void _monitorWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      int reputationcheckcounter = 8;
      int newbadgecheckcounter = 58;
      while (!_monitorWorker.CancellationPending)
      {
        //check every 5 seconds here if a new question has arrived on stack overflow
        Thread.Sleep(5000);
        //TODO: What happens here if a question belongs to several TAG groups. Will the question be displayed several times?
        foreach (string taggroup in TagGroupList)
        {
          var newquestions = datamanager.GetNewQuestions(questionlastchecktime, taggroup);
          if (newquestions != null)
          {
            foreach (var question in newquestions)
            {
              if (question.TimeStamp > questionlastchecktime) questionlastchecktime = question.TimeStamp + 1;
              _monitorWorker.ReportProgress((int)NotificationType.NewQuestion, question);
            }
          }
        }
        if (reputationcheckcounter >= 10 && ConnectedUser != null)
        {
          reputationcheckcounter = 0;
          var newreputationchanges = datamanager.GetReputationChanges(ConnectedUser.Id, reputationlastchecktime);
          if (newreputationchanges != null)
          {
            foreach (var reputationchange in newreputationchanges)
            {
              if (reputationchange.TimeStamp >= reputationlastchecktime) reputationlastchecktime = reputationchange.TimeStamp + 1;
              _monitorWorker.ReportProgress((int)NotificationType.ReputationChange, reputationchange);
            }
            if (newreputationchanges.Count > 0)
              _monitorWorker.ReportProgress((int)NotificationType.UpdateUser,
                                            datamanager.PopulateUserInformation(ConnectedUser.Id, 128));
          }
        }
        reputationcheckcounter++;
        if (newbadgecheckcounter >= 60 && ConnectedUser != null)
        {
          newbadgecheckcounter = 0;
          var newbadgechanges = datamanager.GetNewBadges(ConnectedUser.Id, newbadgeschecktime);
          if (newbadgechanges != null)
          {
            foreach (var newbadgechange in newbadgechanges)
            {
              if (newbadgechange.TimeStamp >= newbadgeschecktime) newbadgeschecktime = newbadgechange.TimeStamp + 1;
              _monitorWorker.ReportProgress((int)NotificationType.NewBadge, newbadgechange);
            }
            if (newbadgechanges.Count > 0)
              _monitorWorker.ReportProgress((int)NotificationType.UpdateUser,
                                            datamanager.PopulateUserInformation(ConnectedUser.Id, 128));
          }
        }
        newbadgecheckcounter++;
      }
    }

    void _monitorWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      switch ((NotificationType)e.ProgressPercentage)
      {
        case NotificationType.NewQuestion:
          var notificator = new MonitorQuestionNotificator(e.UserState as Question);
          notificator.Show();
          break;
        case NotificationType.ReputationChange:
          var repnotificator = new MonitorReputationNotificator(e.UserState as Reputation);
          repnotificator.Show();
          break;
        case NotificationType.NewBadge:
          var badgenotificator = new MonitorBadgeNotificator(e.UserState as Badge);
          badgenotificator.Show();
          break;
        case NotificationType.UpdateUser:
          ConnectedUser = e.UserState as User;
          break;
      }
    }

    private void mainApplicationMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
      if (ConnectedUser != null && e.ClickedItem.Text == ConnectedUser.Name)
      {
        //open the profile page for the connected user
        Process.Start("http://www.stackoverflow.com/users/" + ConnectedUser.Id);
      }
      else if(e.ClickedItem.Text.ToLower().Contains("reputation"))
      {
        Process.Start("http://stackoverflow.com/reputation");
      }
      else if (e.ClickedItem.Text.ToLower().Contains("badges"))
      {
        Process.Start("http://stackoverflow.com/badges");
      }
      else
      {
        switch (e.ClickedItem.Text.ToLower())
        {
          case "exit":
            NotificationIcon.Visible = false;
            SaveSettings();
            Application.Exit();
            break;
          case "about":
            //show about window here
            break;
          case "preferences":
            var preferences = new MonitorPreferences(this);
            preferences.Show();
            break;
        }
      }
    }

    private void SaveSettings()
    {
      if (_connectedUser != null)
      {
        Settings.Default.ConnectedUser = _connectedUser.Id;
      }
      else
      {
        Settings.Default.ConnectedUser = -1;
      }
      var taggroupstring = new StringBuilder();
      TagGroupList.ForEach(s => taggroupstring.Append(s + "¤"));
      Settings.Default.TagGroupList = taggroupstring.ToString();
      Settings.Default.LastBadgesCheckTime = newbadgeschecktime;
      Settings.Default.LastReputationCheckTime = reputationlastchecktime;
      Settings.Default.NewQuestionTimeout = MonitorQuestionNotificator.DisplayTime;
      Settings.Default.ReputationChangeTimeout = MonitorReputationNotificator.DisplayTime;
      Settings.Default.NewBadgeTimeout = MonitorBadgeNotificator.DisplayTime;
      Settings.Default.Save();
    }

    private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      MonitorAnimator.CloseAllAnimatedForms();
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      AboutStackMonitor aboutbox = new AboutStackMonitor();
      aboutbox.ShowDialog();
    }
  }
}
