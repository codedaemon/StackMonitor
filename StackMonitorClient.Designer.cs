namespace StackMonitor {
  partial class StackMonitorClient {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StackMonitorClient));
      this.NotificationIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.mainApplicationMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.connectedUserMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.reputationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.badgesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.mainSeperator = new System.Windows.Forms.ToolStripSeparator();
      this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.mainApplicationMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // NotificationIcon
      // 
      this.NotificationIcon.ContextMenuStrip = this.mainApplicationMenu;
      this.NotificationIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotificationIcon.Icon")));
      this.NotificationIcon.Text = "StackMonitor";
      this.NotificationIcon.Visible = true;
      // 
      // mainApplicationMenu
      // 
      this.mainApplicationMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectedUserMenuItem,
            this.reputationMenuItem,
            this.badgesMenuItem,
            this.mainSeperator,
            this.closeAllToolStripMenuItem,
            this.preferencesToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.exitToolStripMenuItem});
      this.mainApplicationMenu.Name = "mainApplicationMenu";
      this.mainApplicationMenu.Size = new System.Drawing.Size(188, 186);
      this.mainApplicationMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.mainApplicationMenu_ItemClicked);
      // 
      // connectedUserMenuItem
      // 
      this.connectedUserMenuItem.Name = "connectedUserMenuItem";
      this.connectedUserMenuItem.Size = new System.Drawing.Size(187, 22);
      this.connectedUserMenuItem.Text = "Not connected";
      // 
      // reputationMenuItem
      // 
      this.reputationMenuItem.Name = "reputationMenuItem";
      this.reputationMenuItem.Size = new System.Drawing.Size(187, 22);
      this.reputationMenuItem.Text = "Reputation:";
      this.reputationMenuItem.Visible = false;
      // 
      // badgesMenuItem
      // 
      this.badgesMenuItem.Name = "badgesMenuItem";
      this.badgesMenuItem.Size = new System.Drawing.Size(187, 22);
      this.badgesMenuItem.Text = "Badges:";
      this.badgesMenuItem.Visible = false;
      // 
      // mainSeperator
      // 
      this.mainSeperator.Name = "mainSeperator";
      this.mainSeperator.Size = new System.Drawing.Size(184, 6);
      // 
      // closeAllToolStripMenuItem
      // 
      this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
      this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
      this.closeAllToolStripMenuItem.Text = "Close all notifications";
      this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
      // 
      // preferencesToolStripMenuItem
      // 
      this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
      this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
      this.preferencesToolStripMenuItem.Text = "Preferences";
      // 
      // aboutToolStripMenuItem
      // 
      this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
      this.aboutToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
      this.aboutToolStripMenuItem.Text = "About";
      this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
      this.exitToolStripMenuItem.Text = "Exit";
      // 
      // StackMonitorClient
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(234, 62);
      this.Name = "StackMonitorClient";
      this.ShowInTaskbar = false;
      this.Text = "StackMonitorClient";
      this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
      this.mainApplicationMenu.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.NotifyIcon NotificationIcon;
    private System.Windows.Forms.ContextMenuStrip mainApplicationMenu;
    private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem connectedUserMenuItem;
    private System.Windows.Forms.ToolStripSeparator mainSeperator;
    private System.Windows.Forms.ToolStripMenuItem reputationMenuItem;
    private System.Windows.Forms.ToolStripMenuItem badgesMenuItem;
    private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
  }
}