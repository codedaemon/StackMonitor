using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

namespace StackMonitor
{
  public class MonitorAnimator
  {

    static int TotalNotificatorHeight { get; set; }
    static List<MonitorAnimator> AnimatedForms { get; set; }
    public static bool ActiveForms { get { return AnimatedForms.Count > 0; } }
    enum AnimationFlags
    {
      AnimateUp,
      AnimateDown,
      FadeOut
    }

    BackgroundWorker _animationWorker = new BackgroundWorker();
    Form _formToAnimate;
    int _yofset;

    static MonitorAnimator()
    {
      TotalNotificatorHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
      AnimatedForms = new List<MonitorAnimator>();
    }

    public MonitorAnimator(Form form)
    {
      AnimatedForms.Add(this);
      _formToAnimate = form;
      form.Opacity = 0D;
      _yofset = TotalNotificatorHeight;
      TotalNotificatorHeight += (form.Height + 5);
      form.Location = new Point(Screen.PrimaryScreen.Bounds.Right - form.Bounds.Width, Screen.PrimaryScreen.Bounds.Height - _yofset);
      _animationWorker.DoWork += _animationWorker_DoWork;
      _animationWorker.WorkerReportsProgress = true;
      _animationWorker.ProgressChanged += new ProgressChangedEventHandler(_animationWorker_ProgressChanged);
    }

    public static void CloseAllAnimatedForms()
    {
      AnimatedForms.ForEach( f => f.FadeOut());
      TotalNotificatorHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
    }

    public void ScrollUp()
    {
      if (!_animationWorker.IsBusy)
        _animationWorker.RunWorkerAsync(AnimationFlags.AnimateUp);
    }
    public void ScrollDown()
    {
      if (!_animationWorker.IsBusy)
        _animationWorker.RunWorkerAsync(AnimationFlags.AnimateDown);
    }
    public void FadeOut()
    {
      if (!_animationWorker.IsBusy)
        _animationWorker.RunWorkerAsync(AnimationFlags.FadeOut);
    }

    void _animationWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      if (e.UserState is double)
      {
        _formToAnimate.Opacity = (double)e.UserState;
        if (_formToAnimate.Opacity == 0D)
        {
          if (TotalNotificatorHeight >= _formToAnimate.Height) TotalNotificatorHeight -= (_formToAnimate.Height + 5);
          AnimatedForms.Remove(this);
          _formToAnimate.Close();
        }
      }
      else if (e.UserState is Point)
      {
        //calculate the transparency for the panel here
        double opacity = (Screen.PrimaryScreen.Bounds.Height - _formToAnimate.Location.Y - _yofset) / (double)(_formToAnimate.Size.Height * 1.1);
        _formToAnimate.Opacity = opacity;
        //then update the position
        _formToAnimate.Location = (Point)e.UserState;
      }
    }

    void _animationWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      if (!(e.Argument is AnimationFlags))
      {
        return;
      }
      long ticks1 = 0;
      long ticks2 = 0;
      double interval = (double)Stopwatch.Frequency / 60;
      bool looping = true;
      while (looping)
      {
        ticks2 = Stopwatch.GetTimestamp();
        if (ticks2 >= ticks1 + interval)
        {
          ticks1 = Stopwatch.GetTimestamp();
          switch ((AnimationFlags)e.Argument)
          {
            case AnimationFlags.AnimateUp:
              looping = AnimateUp(); break;
            case AnimationFlags.AnimateDown:
              looping = AnimateDown(); break;
            case AnimationFlags.FadeOut:
              looping = AnimateFadeOut(); break;
            default:
              looping = false; break;
          }
        }
        Thread.Sleep(1);
      }
    }

    private bool AnimateDown()
    {
      //Move the window downwards
      Point location = new Point(_formToAnimate.Location.X, _formToAnimate.Location.Y + 10);
      if (location.Y > Screen.PrimaryScreen.Bounds.Height - _yofset)
      {
        location.Y = Screen.PrimaryScreen.Bounds.Height - _yofset;
        _animationWorker.ReportProgress(0, 0D);
        return false;
      }
      _animationWorker.ReportProgress(0, location);
      return true;
    }

    private bool AnimateUp()
    {
      //Move the window upwards
      Point location = new Point(_formToAnimate.Location.X, _formToAnimate.Location.Y - 10);
      if (location.Y + _formToAnimate.Height < Screen.PrimaryScreen.Bounds.Height - _yofset)
      {
        location.Y = Screen.PrimaryScreen.Bounds.Height - _formToAnimate.Height - _yofset;
        _animationWorker.ReportProgress(0, location);
        return false;
      }
      _animationWorker.ReportProgress(0, location);
      return true;
    }

    private bool AnimateFadeOut()
    {
      //fade the form out until it reaches 0 alpha, then delete it.
      if (_formToAnimate.Opacity > 0.1D)
      {
        _animationWorker.ReportProgress(0, _formToAnimate.Opacity - 0.1D);
        return true;
      }
      _animationWorker.ReportProgress(0, 0D);
      return false;
    }


  }
}
