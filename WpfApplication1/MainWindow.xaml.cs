﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Shapes;

namespace WpfApplication1
{
	public partial class MainWindow : Window
	{
        DispatcherTimer _timer = new DispatcherTimer();
        DispatcherTimer _Menutimer = new DispatcherTimer();

		public MainWindow()
		{
			this.InitializeComponent();
            bool fullscreen = false;
            mediaControl.Volume = (double)volumeSlider.Value;
            _timer.Interval = TimeSpan.FromMilliseconds(900);
            _timer.Tick += new EventHandler(timer_Tick);
            _Menutimer.Interval = TimeSpan.FromMilliseconds(3000);
            _Menutimer.Tick += new EventHandler(timer_Menu);
            _timer.Start();
            mediaControl.MouseLeftButtonDown += delegate
            {
                if (!fullscreen)
                {
                    FullScreenBehavior.SetIsFullScreen(Window, true);
                }
                else
                {
                    _Menutimer.Stop();
                    timerMenu = true;
                    slider2.Height = 24;
                    Panel.Height = 73;
                    FullScreenBehavior.SetIsFullScreen(Window, false);
                }
                fullscreen = !fullscreen;
            };
		}

        TimeSpan ts2;
        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaControl.NaturalDuration.HasTimeSpan)
            {
                ts2 = mediaControl.NaturalDuration.TimeSpan;
                slider2.Maximum = ts2.TotalSeconds;
                slider2.SmallChange = 1;
                slider2.LargeChange = Math.Min(10, ts2.Seconds / 10);
            }
            _timer.Start();
        }

        bool isDragging = false;
        double currentposition = 0;

        void timer_Tick(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                slider2.Value = mediaControl.Position.TotalSeconds;
                currentposition = slider2.Value;
            }
        }

        void timer_Menu(object sender, EventArgs e)
        {
            Panel.Height = 0;
            slider2.Height = 0;
            //Cursor.Hide();
            _Menutimer.Stop();
            timerMenu = true;
        }

        private void seekBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void seekBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDragging = false;
            mediaControl.Position = TimeSpan.FromSeconds(slider2.Value);
        }

        bool play = false;
        private void play_Click(object sender, RoutedEventArgs e)
        {
            if (!play)
            {
                mediaControl.Play();
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri("/MediaPlayer;component/Images/pause.png", UriKind.Relative);
                bi3.EndInit();
                ImagePlay.Source = bi3;
            }
            else
            {
                mediaControl.Pause();
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri("/MediaPlayer;component/Images/play.png", UriKind.Relative);
                bi3.EndInit();
                ImagePlay.Source = bi3;
            }
            play = !play;
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            mediaControl.Pause();
            button.IsEnabled = true;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            mediaControl.Stop();
        }

        private void load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog os = new OpenFileDialog();
            os.AddExtension = true;
            os.DefaultExt = "*.*";
            os.Filter = "Media (*.*)|*.*";
            os.ShowDialog();
            if (os.FileName != "")
            {
                mediaControl.Source = new Uri(os.FileName);
                Window.Height = 319;
                play_Click(this, e);
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaControl.Volume = (double)volumeSlider.Value;
        }

        bool sound_on = false;
        private void sound_Click(object sender, RoutedEventArgs e)
        {
            if (!sound_on)
            {
                mediaControl.Volume = 0;
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri("/MediaPlayer;component/Images/soundoff.png", UriKind.Relative);
                bi3.EndInit();
                ImageSound.Source = bi3;
            }
            else
            {
                mediaControl.Volume = (double)volumeSlider.Value;
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri("/MediaPlayer;component/Images/soundon.png", UriKind.Relative);
                bi3.EndInit();
                ImageSound.Source = bi3;
            }
            sound_on = !sound_on;
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan ts = mediaControl.Position;
            if (ts.Seconds < 10 && ts.Minutes < 10)
                time.Text = ts.Hours + ":0" + ts.Minutes + ":0" + ts.Seconds;
            else if (ts.Seconds < 10 && ts.Minutes >= 10)
                time.Text = ts.Hours + ":" + ts.Minutes + ":0" + ts.Seconds;
            else if (ts.Seconds >= 10 && ts.Minutes < 10)
                time.Text = ts.Hours + ":0" + ts.Minutes + ":" + ts.Seconds;
            else if (ts.Seconds >= 10 && ts.Minutes >= 10)
                time.Text = ts.Hours + ":" + ts.Minutes + ":" + ts.Seconds;
            if (ts2.Seconds < 10 && ts2.Minutes < 10)
                time.Text += "/" + ts2.Hours + ":0" + ts2.Minutes + ":0" + ts2.Seconds;
            else if (ts2.Seconds < 10 && ts2.Minutes >= 10)
                time.Text += "/" + ts2.Hours + ":" + ts2.Minutes + ":0" + ts2.Seconds;
            else if (ts2.Seconds >= 10 && ts2.Minutes < 10)
                time.Text += "/" + ts2.Hours + ":0" + ts2.Minutes + ":" + ts.Seconds;
            else if (ts2.Seconds >= 10 && ts2.Minutes >= 10)
                time.Text += "/" + ts2.Hours + ":" + ts2.Minutes + ":" + ts2.Seconds;
        }

        bool timerMenu = true;
        private void mediaControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (timerMenu && Window.Width > 500)
            {
                slider2.Height = 24;
                Panel.Height = 73;
                timerMenu = false;
                _Menutimer.Start();
            }
        }

        private void slider2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mediaControl.Position = TimeSpan.FromSeconds(slider2.Value);
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string filename = (string)((System.Windows.DataObject)e.Data).GetFileDropList()[0];
            mediaControl.Source = new Uri(filename);
            mediaControl.LoadedBehavior = MediaState.Manual;
            mediaControl.UnloadedBehavior = MediaState.Manual;
            mediaControl.Volume = (double)volumeSlider.Value;
            Window.Height = 319;
            play_Click(this, e);
            mediaControl.Play();
        }

	}
}