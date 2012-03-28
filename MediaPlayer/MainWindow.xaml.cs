using System;
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
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace MediaPlayer
{
    class Subtitle
    {
        public TimeSpan Begin;
        public TimeSpan End;
        public string text;
    }

	public partial class MainWindow : Window
	{
        DispatcherTimer _timer = new DispatcherTimer();
        DispatcherTimer _menutimer = new DispatcherTimer();
        DispatcherTimer _srtTimer = new DispatcherTimer();

        StyleSubtitle   _windowSubtitle;
        bool            _timerMenu = true;
        String[]        _lineSubtitle = new String[10000];
        List<Subtitle>  _subtitles = new List<Subtitle>();

        public double currentTimeSubtitle;

		public MainWindow()
		{
			this.InitializeComponent();
            _windowSubtitle = new StyleSubtitle(this);
            _windowSubtitle.Hide();
            mediaControl.Volume = (double)volumeSlider.Value/50;
            _timer.Interval = TimeSpan.FromMilliseconds(900);
            _timer.Tick += new EventHandler(timer_Tick);
            _menutimer.Interval = TimeSpan.FromMilliseconds(1300);
            _menutimer.Tick += new EventHandler(timer_Menu);
            _timerOpacity.Interval = TimeSpan.FromMilliseconds(100);
            _timerOpacity.Tick += new EventHandler(timer_Opacity);
            _srtTimer.Interval = TimeSpan.FromMilliseconds(100);
            _srtTimer.Tick += new EventHandler(check_Srt);
            _srtTimer.Start();
            _timer.Start();
		}

        TimeSpan ts2;

        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaControl.NaturalDuration.HasTimeSpan)
            {
                ts2 = mediaControl.NaturalDuration.TimeSpan;
                slider2.Maximum = ts2.TotalSeconds;
                slider2.SmallChange = 1;
                slider2.LargeChange =  Math.Min(10, ts2.Seconds / 10);
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

        DispatcherTimer _timerOpacity = new DispatcherTimer();
        void timer_Menu(object sender, EventArgs e)
        {
            if (ContextMenu != true)
            {
                _timerOpacity.Start();
                Panel.Opacity = 1;
                _menutimer.Stop();
                _timerMenu = true;
            }
        }

        void timer_Opacity(object sender, EventArgs e)
        {
            if (Panel.Opacity <= 0)
            {
                _timerOpacity.Stop();
                Mouse.OverrideCursor = System.Windows.Input.Cursors.None;
            }
            Panel.Opacity -= 0.05;
        }

        private void seekBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
            Subtitle.Text = "";
        }

        private void seekBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDragging = false;
            mediaControl.Position = TimeSpan.FromSeconds(slider2.Value);
        }

        public bool play = false;
        public void play_Click(object sender, RoutedEventArgs e)
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
            play = false;
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri("/MediaPlayer;component/Images/play.png", UriKind.Relative);
            bi3.EndInit();
            ImagePlay.Source = bi3;
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
                _subtitles.Clear();
                Subtitle.Text = "";
                loadSubtitle(os.FileName, false);
                Window.Height = 319;
                play = false;
                play_Click(this, e);
            }
        }

        public void loadSubtitle(String Filename, bool isSrt)
        {
            try
            {
                if (isSrt != true)
                {
                    char delimiterChars = '.';
                    string[] words = Filename.Split(delimiterChars);
                    string File = words[0] += ".";
                    for (int x = 1; x < words.Length - 1; x++)
                        File += words[x] += ".";
                    using (StreamReader sr = new StreamReader(File + "srt", System.Text.Encoding.Default))
                    {
                        String line;
                        for (int x = 0; (line = sr.ReadLine()) != null; x++)
                            _lineSubtitle[x] = line;
                        parse_Srt(_lineSubtitle);
                    }
                }
                else
                    using (StreamReader sr = new StreamReader(Filename, System.Text.Encoding.Default))
                    {
                        String line;
                        for (int x = 0; (line = sr.ReadLine()) != null; x++)
                            _lineSubtitle[x] = line;
                        parse_Srt(_lineSubtitle);
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0}", e.Message);
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaControl.Volume = (double)volumeSlider.Value/50;
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
                mediaControl.Volume = (double)volumeSlider.Value/50;
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
                timeend.Text = ts2.Hours + ":0" + ts2.Minutes + ":0" + ts2.Seconds;
            else if (ts2.Seconds < 10 && ts2.Minutes >= 10)
                timeend.Text = ts2.Hours + ":" + ts2.Minutes + ":0" + ts2.Seconds;
            else if (ts2.Seconds >= 10 && ts2.Minutes < 10)
                timeend.Text = ts2.Hours + ":0" + ts2.Minutes + ":" + ts2.Seconds;
            else if (ts2.Seconds >= 10 && ts2.Minutes >= 10)
                timeend.Text = ts2.Hours + ":" + ts2.Minutes + ":" + ts2.Seconds;
        }

        private void check_Srt(object sender, EventArgs e)
        {
            TimeSpan ts = mediaControl.Position;
            for (int x = 0; x < _subtitles.Count; x++)
            {
                if (_subtitles[x].Begin.Hours == ts.Hours && _subtitles[x].Begin.Minutes == ts.Minutes && _subtitles[x].Begin.Seconds == ts.Seconds)
                    Subtitle.Text = decodeHTML(_subtitles[x].text);
                else if (_subtitles[x].End.Hours == ts.Hours && _subtitles[x].End.Minutes == ts.Minutes && _subtitles[x].End.Seconds == ts.Seconds)
                    Subtitle.Text = "";
            }
        }


        private String decodeHTML(String line)
        {
            char[] delimiterChars = { '>' , '<' };
            string[] tmp = line.Split(delimiterChars);
            if (tmp.Length > 1)
            {
                if (tmp[1] == "i" && tmp[tmp.Length - 2] == "/i")
                    Subtitle.FontStyle = FontStyles.Italic;
                else if (tmp[1] == "b" && tmp[tmp.Length - 2] == "/b")
                    Subtitle.FontStyle = FontStyles.Oblique;
                string text = tmp[2];
                for (int x = 3; x < tmp.Length - 2; x++)
                {
                    if (tmp[x] == "/i" || tmp[x] == "/b")
                        x++;
                    text += tmp[x];
                }
                return text;
            }
            else
            {
                Subtitle.FontStyle = FontStyles.Normal;
                return line;
            }
        }
        private void parse_Srt(String[] text)
        {
            int currentLineTime = 1;
            for (int currentLineText = 2; text[currentLineTime] != null; currentLineText += 4)
            {
                Subtitle sub = new Subtitle();
                string[] Timers = Regex.Split(text[currentLineTime], " --> ");
                char[] delimiterChars = { ':', ',' };
                string[] TimeBegin = Timers[0].Split(delimiterChars);
                string[] TimeEnd = Timers[1].Split(delimiterChars);
                sub.Begin = new TimeSpan(0, int.Parse(TimeBegin[0]), int.Parse(TimeBegin[1]), int.Parse(TimeBegin[2]), int.Parse(TimeBegin[3]));
                sub.End = new TimeSpan(0, int.Parse(TimeEnd[0]), int.Parse(TimeEnd[1]), int.Parse(TimeEnd[2]), int.Parse(TimeEnd[3]));
                sub.text = text[currentLineText];
                if (_lineSubtitle[currentLineText + 1].Length >= 2)
                {
                    sub.text += "\n" + text[currentLineText+1];
                    currentLineTime++;
                    currentLineText++;
                }
                currentLineTime += 4;
                _subtitles.Add(sub);
            }
        }

        private void mediaControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _timerOpacity.Stop();
            Panel.Opacity = 1;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            if (_timerMenu && Window.Width > 500)
            {
                slider2.Height = 24;
                _timerMenu = false;
                _menutimer.Start();
            }
        }

        private void slider2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mediaControl.Position = TimeSpan.FromSeconds(slider2.Value);
            Subtitle.Text = "";
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string filename = (string)((System.Windows.DataObject)e.Data).GetFileDropList()[0];
            string[] words = filename.Split('.');
            if (words[words.Length - 1] == "srt")
                loadSubtitle(filename, true);
            else
            {
                mediaControl.Source = new Uri(filename);
                mediaControl.LoadedBehavior = MediaState.Manual;
                mediaControl.UnloadedBehavior = MediaState.Manual;
                mediaControl.Volume = (double)volumeSlider.Value / 50;
                Window.Height = 319;
                play = false;
                play_Click(this, e);
                mediaControl.Play();
            }
        }

        bool ContextMenu = false;
        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu = true;
            this.contextMenu.PlacementTarget = sender as UIElement;
            this.contextMenu.IsOpen = true;
            Panel.Opacity = 1;
            _timerOpacity.Stop();
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public void setFullscreen()
        {
            if (!fullscreen)
            {
                FullScreenBehavior.SetIsFullScreen(Window, true);
                TopMenuPanel.Opacity = 0;
                Subtitle.FontSize = 21;
                Subtitle.Margin = new Thickness(10, 0, -10, 80);
            }
            else
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
                _menutimer.Stop();
                _timerMenu = true;
                slider2.Height = 24;
                Panel.Height = 76;
                Panel.Opacity = 1;
                _timerOpacity.Stop();
                _menutimer.Stop();
                FullScreenBehavior.SetIsFullScreen(Window, false);
                TopMenuPanel.Opacity = 0.9;
                Subtitle.FontSize = 12;
                Subtitle.Margin = new Thickness(10, 0, -10, 60);
            }
            fullscreen = !fullscreen;
        }

        bool fullscreen = false;
        private void mediaControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                setFullscreen();
            }
        }

        private void Subtitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                setFullscreen();
            }
        }

        private void OpenUrl_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl.IsEnabled = true;
            Window1 WOpenUrl = new Window1(this);
            WOpenUrl.Show();

        }

        private void Open_Click(object sender, RoutedEventArgs e)
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
                 play = false;
                play_Click(this, e);
            }
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }

        private void Panel_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            _timerOpacity.Stop();
            Panel.Opacity = 1;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu = false;
        }

        private void OpenSubtitle(object sender, RoutedEventArgs e)
        {
            OpenFileDialog os = new OpenFileDialog();
            os.AddExtension = true;
            os.DefaultExt = "*.*";
            os.Filter = "Media (*.*)|*.*";
            os.ShowDialog();
            if (os.FileName != "")
               loadSubtitle(os.FileName, true);
        }

        private void StyleSubtitle(object sender, RoutedEventArgs e)
        {
            if (!_windowSubtitle.IsVisible)
            {
                _windowSubtitle = new StyleSubtitle(this);
                _windowSubtitle.Show();
            }
        }

        private void MFullscreen_Click(object sender, RoutedEventArgs e)
        {
            setFullscreen();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _windowSubtitle.Close();
        }

	}
}