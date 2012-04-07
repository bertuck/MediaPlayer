using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl
    {

        ObservableCollection<PlaylistInfo> PlaylistCollection =
        new ObservableCollection<PlaylistInfo>();

        string _total;
        TagLib.File tagFile;
        bool play = false;
        int idPlaylist;
        int curPlaylist;
        int curPlay;
        public string PlaylistName { get; set; }
        public static readonly RoutedEvent SelectedEvent =
        EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble,
        typeof(RoutedEventHandler), typeof(Playlist));

        public Playlist()
        {
            idPlaylist = 0;
            curPlaylist = 0;
            curPlay = 0;
            InitializeComponent();

            PlaylistCollection.Insert(idPlaylist, new PlaylistInfo { Name = "Current list", id = 0, cur = 0, timeInit = 0 });
            idPlaylist++;
            titre.Text = PlaylistCollection.ElementAt(curPlaylist).Name;
            labelTotal.Content = Total();
            liste.ItemsSource = SoundCollection;
            Selection.ItemsSource = PlaylistCollection;
            // SearchSelections();

        }
        /*
        ~Playlist()
        {
            SaveSelections();
        }
        */
        public string Total()
        {
            _total = "Total: ";
            double h = 0;
            double m = 0;
            double sec = 0;
            foreach (SoundInfo s in SoundCollection)
            {
                h += s.H;
                m += s.M;
                sec += s.S;
            }
            while (sec >= 60)
            {
                m++;
                sec -= 60;
            }
            while (m >= 60)
            {
                h++;
                m -= 60;
            }
            if (h == 0 && m == 0 && sec == 0)
            {
                _total += "--:--";
                return _total;
            }
            if (h > 0)
            {
                if (h < 10)
                {
                    _total += "0";
                }
                _total += h + ":";

            }
            if (m < 10)
            {
                _total += "0";
            }
            _total += m + ":";
            if (sec < 10)
            {
                _total += "0";
            }
            _total += sec;

            return _total;
        }
        public void SearchPlaylist(string sPath)
        {

            PlaylistCollection.Insert(idPlaylist, new PlaylistInfo { Name = System.IO.Path.GetFileNameWithoutExtension(sPath), path = sPath, id = 0, cur = 0, timeInit = 0 });
            PlaylistInfo p = PlaylistCollection.ElementAt(idPlaylist);
            curPlaylist = idPlaylist;
            idPlaylist++;
            string[] lines = System.IO.File.ReadAllLines(System.IO.Path.GetFullPath(sPath));
            foreach (string line in lines)
            {
                AddSound(line);
            }
            titre.Text = PlaylistCollection.ElementAt(curPlaylist).Name;
            liste.ItemsSource = SoundCollection;
            labelTotal.Content = Total();
        }
        public event RoutedEventHandler Selected
        {
            add
            {
                this.AddHandler(SelectedEvent, value);
            }
            remove
            {
                this.RemoveHandler(SelectedEvent, value);
            }
        }

        public ObservableCollection<SoundInfo> SoundCollection
        { get { return (PlaylistCollection.ElementAt(curPlaylist)).SoundCollection; } }

        public void AddUrl(string filename)
        {
            SoundInfo s = new SoundInfo
            {
                SoundName = filename,
                FileName = filename,

            };
            s.media = new MediaElement();
            s.media.LoadedBehavior = MediaState.Manual;
            s.media.UnloadedBehavior = MediaState.Manual;
            s.media.Source = new Uri(filename, UriKind.Absolute);
            s.media.Pause();
            s.media.MediaOpened += new RoutedEventHandler(Opened);

            if ((PlaylistCollection.ElementAt(curPlaylist)).id == (PlaylistCollection.ElementAt(curPlaylist)).cur && play)
                s.Icone = "/MediaPlayer;component/Images/playing.png";
            SoundCollection.Insert((PlaylistCollection.ElementAt(curPlaylist)).id, s);
            (PlaylistCollection.ElementAt(curPlaylist)).id++;
        }

        public void AddSound(string filename)
        {
            if (System.IO.Path.GetExtension(filename) == ".m3u")
                SearchPlaylist(filename);
            else
            {
                tagFile = TagLib.File.Create(filename);

                string name = tagFile.Tag.Title;
                if (tagFile.Tag.Title == null)
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(filename);

                }
                SoundInfo s = new SoundInfo
                {
                    SoundName = name,
                    FileName = filename,
                    Creator = tagFile.Tag.FirstArtist

                };
                s.media = new MediaElement();
                s.media.LoadedBehavior = MediaState.Manual;
                s.media.UnloadedBehavior = MediaState.Manual;
                s.media.Source = new Uri(filename);
                s.media.Pause();
                s.media.MediaOpened += new RoutedEventHandler(Opened);

                if ((PlaylistCollection.ElementAt(curPlaylist)).id == (PlaylistCollection.ElementAt(curPlaylist)).cur && play && curPlaylist == curPlay)
                    s.Icone = "/MediaPlayer;component/Images/playing.png";
                SoundCollection.Insert((PlaylistCollection.ElementAt(curPlaylist)).id, s);
                (PlaylistCollection.ElementAt(curPlaylist)).id++;
            }
        }
        private void Opened(object sender, RoutedEventArgs e)
        {
            if (((MediaElement)sender).NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = ((MediaElement)sender).NaturalDuration.TimeSpan;
                Debug.WriteLine((PlaylistCollection.ElementAt(curPlaylist)).timeInit);
                Debug.WriteLine(curPlaylist);

                SoundInfo s = SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlaylist)).timeInit);
                SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlaylist)).timeInit);
                s.S = ts.Seconds;
                s.M = ts.Minutes;
                s.H = ts.Hours;
                Debug.WriteLine("Time: " + ts.TotalSeconds);
                SoundCollection.Insert((PlaylistCollection.ElementAt(curPlaylist)).timeInit, s);
            }
            (PlaylistCollection.ElementAt(curPlaylist)).timeInit++;
            labelTotal.Content = Total();
        }

        public void NextSound()
        {
            if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
            {
                SoundInfo p = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                p.Icone = "";
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p);
                (PlaylistCollection.ElementAt(curPlay)).cur++;
                if ((PlaylistCollection.ElementAt(curPlay)).cur >= (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count)
                    (PlaylistCollection.ElementAt(curPlay)).cur = 0;
                SoundInfo s = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                if (play)
                    s.Icone = "/MediaPlayer;component/Images/playing.png";
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, s);
                curPlaylist = curPlay;
                titre.Text = PlaylistCollection.ElementAt(curPlaylist).Name;
                liste.ItemsSource = SoundCollection;
                labelTotal.Content = Total();

                liste.SelectedItem = s;
                RoutedEventArgs newEventArgs = new RoutedEventArgs(SelectedEvent);
                RaiseEvent(newEventArgs);
            }
        }
        public void PrevSound()
        {
            if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
            {
                SoundInfo p = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                p.Icone = "";
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p);
                (PlaylistCollection.ElementAt(curPlay)).cur--;
                if ((PlaylistCollection.ElementAt(curPlay)).cur < 0)
                    (PlaylistCollection.ElementAt(curPlay)).cur = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count - 1;
                SoundInfo s = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                if (play)
                    s.Icone = "/MediaPlayer;component/Images/playing.png";
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, s);
                curPlaylist = curPlay;
                titre.Text = PlaylistCollection.ElementAt(curPlaylist).Name;
                liste.ItemsSource = SoundCollection;
                labelTotal.Content = Total();

                liste.SelectedItem = s;
                RoutedEventArgs newEventArgs = new RoutedEventArgs(SelectedEvent);
                RaiseEvent(newEventArgs);
            }
        }

        public void Play()
        {
            play = true;
            if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count() > 0)
            {
                SoundInfo p = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                p.Icone = "/MediaPlayer;component/Images/playing.png";
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p);
            }

        }
        public void Pause()
        {
            play = false;
            if (SoundCollection.Count() > 0)
            {
                SoundInfo p = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                p.Icone = "";
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p);
            }
        }

        private void liste_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            SoundInfo s = (SoundInfo)liste.SelectedItem;
            if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
            {
                SoundInfo p = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                p.Icone = "";
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p);
                (PlaylistCollection.ElementAt(curPlaylist)).cur = SoundCollection.IndexOf(s);
            }
            curPlay = curPlaylist;
            if (play)
                s.Icone = "/MediaPlayer;component/Images/playing.png";
            SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlaylist)).cur);
            SoundCollection.Insert((PlaylistCollection.ElementAt(curPlaylist)).cur, s);
            liste.SelectedItem = s;
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SelectedEvent);
            RaiseEvent(newEventArgs);
        }

        public void refreshIcon(string filename)
        {
            SoundInfo s = null;
            foreach (SoundInfo so in SoundCollection)
            {
                if (so.FileName == filename)
                {
                    s = so;
                    break;
                }
            }
            if (s == null)
                return;
            if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
            {
                SoundInfo p = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                p.Icone = "";
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p);
                (PlaylistCollection.ElementAt(curPlaylist)).cur = SoundCollection.IndexOf(s);
            }
            curPlay = curPlaylist;
            if (play)
                s.Icone = "/MediaPlayer;component/Images/playing.png";
            SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlaylist)).cur);
            SoundCollection.Insert((PlaylistCollection.ElementAt(curPlaylist)).cur, s);
            liste.SelectedItem = s;
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SelectedEvent);
            RaiseEvent(newEventArgs);
        }
        private void liste_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SearchSelections()
        {
            if (System.IO.File.Exists("Playlist.plt"))
            {
                System.IO.StreamReader file =
                new System.IO.StreamReader("Playlist.plt");
                string line;
                int counter = 0;
                while ((line = file.ReadLine()) != null)
                {
                    SearchPlaylist(line);
                    counter++;
                }

                file.Close();
            }
        }
        private void SaveSelections()
        {
            string text = "";
            foreach (PlaylistInfo p in PlaylistCollection)
            {
                text += p.path + "\n";
            }
            System.IO.File.WriteAllText("Playlist.plt", text);

        }
        private void save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Playlist"; // Default file name
            dlg.DefaultExt = ".m3u"; // Default file extension
            dlg.Filter = "Playlist (.m3u)|*.m3u"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                string text = "";
                foreach (SoundInfo sound in SoundCollection)
                {
                    text += sound.FileName;
                    text += "\n";
                }
                System.IO.File.WriteAllText(filename, text);
            }

        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            curPlaylist++;
            if (curPlaylist >= idPlaylist)
                curPlaylist = 0;
            titre.Text = PlaylistCollection.ElementAt(curPlaylist).Name;
            liste.ItemsSource = SoundCollection;
            labelTotal.Content = Total();
        }

        private void prev_Click(object sender, RoutedEventArgs e)
        {
            curPlaylist--;
            if (curPlaylist < 0)
                curPlaylist = idPlaylist - 1;
            titre.Text = PlaylistCollection.ElementAt(curPlaylist).Name;
            liste.ItemsSource = SoundCollection;
            labelTotal.Content = Total();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }


    }

    public class PlaylistInfo
    {
        ObservableCollection<SoundInfo> _SoundCollection =
       new ObservableCollection<SoundInfo>();
        public ObservableCollection<SoundInfo> SoundCollection
        { get { return _SoundCollection; } }
        public string Name { get; set; }
        public string path { get; set; }
        public int id;
        public int cur;
        public int timeInit;

    }
    public class SoundInfo
    {
        public string SoundName { get; set; }
        public string FileName { get; set; }
        public string Creator { get; set; }
        public double H { get; set; }
        public double M { get; set; }
        public double S { get; set; }
        public string Icone { get; set; }
        public string Time
        {
            get
            {
                string ret = "";
                if (H > 0)
                {
                    if (H < 10)
                        ret = "0";
                    ret += H + ":";
                }
                if (M < 10)
                    ret += "0";
                ret += M + ":";
                if (S < 10)
                    ret += "0";
                ret += S;

                return ret;
            }
        }

        public MediaElement media { get; set; }
    }

}
