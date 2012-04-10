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
using System.IO;
using System.Data;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl
    {

        ObservableCollection<PlaylistInfo> PlaylistCollection =
        new ObservableCollection<PlaylistInfo>();

        ObservableCollection<Media> ImageCollection =
        new ObservableCollection<Media>();
        ObservableCollection<Media> MusicCollection =
             new ObservableCollection<Media>();
        ObservableCollection<Media> VideoCollection =
             new ObservableCollection<Media>();

     

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
        Library lib = new Library();

        public Playlist()
        {
            idPlaylist = 0;
            curPlaylist = 0;
            curPlay = 0;
            InitializeComponent();

            PlaylistCollection.Insert(idPlaylist, new PlaylistInfo { Name = "Current list", id = 0, cur = 0 });
            idPlaylist++;
            titre.Text = PlaylistCollection.ElementAt(curPlaylist).Name;
            labelTotal.Content = Total();
            liste.ItemsSource = SoundCollection;
            Selection.ItemsSource = PlaylistCollection;
            Video.ItemsSource = VideoCollection;
            Musique.ItemsSource = MusicCollection;
            Image.ItemsSource = ImageCollection;
            SearchSelections();
            init_Library();

        }

        ~Playlist()
        {
            SaveSelections();
            lib.save_To_File();
        }

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
            PlaylistInfo p = null;
            foreach (PlaylistInfo tmp in PlaylistCollection)
            {
                if (tmp.path == sPath)
                {
                    p = tmp;
                    break;
                }
            }

            if (p != null)
            {
                curPlaylist = PlaylistCollection.IndexOf(p);
                titre.Text = p.Name;
                liste.ItemsSource = SoundCollection;
                labelTotal.Content = Total();
            }
            else
            {
                PlaylistCollection.Insert(idPlaylist, new PlaylistInfo { Name = System.IO.Path.GetFileNameWithoutExtension(sPath), path = sPath, id = 0, cur = 0 });
                p = PlaylistCollection.ElementAt(idPlaylist);
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
            SoundInfo s;
            if ((s = isInList(filename)) != null)
            {
                if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
                {
                    SoundInfo p1 = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                    p1.Icone = "";
                    (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                    (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p1);
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
            else
            {
                s = new SoundInfo
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
        }

        public SoundInfo isInList(string filename)
        {
            SoundInfo ret = null;
            foreach (SoundInfo s in SoundCollection)
            {
                if (s.FileName == filename)
                {
                    ret = s;
                    break;
                }
            }
            return ret;
        }

        public void AddSound(string filename)
        {
            SoundInfo s;
            if (System.IO.Path.GetExtension(filename) == ".m3u" && System.IO.File.Exists(filename))
                SearchPlaylist(filename);
            else if (System.IO.File.Exists(filename) && (s = isInList(filename)) != null)
            {
                if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
                {
                    SoundInfo p1 = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                    p1.Icone = "";
                    (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                    (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p1);
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
            else if (System.IO.File.Exists(filename))
            {
                tagFile = TagLib.File.Create(filename);

                string name = tagFile.Tag.Title;
                if (tagFile.Tag.Title == null)
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(filename);

                }
                s = new SoundInfo
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
            else
            {
                string messageBoxText = "Can't find file " + System.IO.Path.GetFileName(filename);
                string caption = "Word Processor";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }
        private void Opened(object sender, RoutedEventArgs e)
        {
            if (((MediaElement)sender).NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = ((MediaElement)sender).NaturalDuration.TimeSpan;
                
                SoundInfo s = null;
                for (int i = 0; i < PlaylistCollection.Count; i++)
                {
                    foreach (SoundInfo med in PlaylistCollection[i].SoundCollection)
                    {
                        if (med.media == (MediaElement)sender)
                        {
                            s = med;
                            int index = PlaylistCollection[i].SoundCollection.IndexOf(s);
                            PlaylistCollection[i].SoundCollection.RemoveAt(index);
                            s.S = ts.Seconds;
                            s.M = ts.Minutes;
                            s.H = ts.Hours;
                            Debug.WriteLine("Time: " + ts.TotalSeconds);
                            Debug.WriteLine("filename: " + s.FileName);

                            PlaylistCollection[i].SoundCollection.Insert(index, s);

                            break;
                        }
                    }
                }
            }
   
            labelTotal.Content = Total();
        }

        private void MediaOpened(object sender, RoutedEventArgs e)
        {
            string time = "";
            MediaElement elem = (MediaElement)sender;
            if (elem.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = elem.NaturalDuration.TimeSpan;
                
                double h = ts.Hours;
                double m = ts.Minutes;
                double sec = ts.Seconds;

                if (h == 0 && m == 0 && sec == 0)
                {
                    time += "--:--";
                }
                if (h > 0)
                {
                    if (h < 10)
                    {
                        time += "0";
                    }
                    time += h + ":";

                }
                if (m < 10)
                {
                    time += "0";
                }
                time += m + ":";
                if (sec < 10)
                {
                    time += "0";
                }
                time += sec;

            }
                Media s = null;
                foreach (Media med in VideoCollection)
                {
                    if (med.media == elem)
                    {
                        s = med;
                        int index = VideoCollection.IndexOf(s);
                        VideoCollection.RemoveAt(index);
                        s.Duration = time;
                        VideoCollection.Insert(index, s);
                  
                        break;
                    }
                }
                if (s == null)
                {
                    foreach (Media med in MusicCollection)
                    {
                        if (med.media == elem)
                        {
                            s = med;

                            int index = MusicCollection.IndexOf(s);
                            MusicCollection.RemoveAt(index);
                            s.Duration = time;
                            MusicCollection.Insert(index, s);
                        
                            break;

                        }
                    }

                }
                if (s == null)
                {
                    foreach (Media med in ImageCollection)
                    {
                        if (med.media == elem)
                        {
                            s = med;
                            int index = ImageCollection.IndexOf(s);
                            ImageCollection.RemoveAt(index);
                            s.Duration = time;
                            ImageCollection.Insert(index, s);
                           
                            break;

                        }
                    }

                }
            
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
            if (s != null)
            {
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
        }
        private void PlaylistChanged(object sender, MouseButtonEventArgs e)
        {
            PlaylistInfo p = (PlaylistInfo)Selection.SelectedItem;
            if (p != null)
            {
                curPlaylist = PlaylistCollection.IndexOf(p);
                titre.Text = PlaylistCollection.ElementAt(curPlaylist).Name;
                liste.ItemsSource = SoundCollection;
                labelTotal.Content = Total();
                if ((PlaylistCollection.ElementAt(curPlaylist)).SoundCollection.Count > 0)
                {
                    if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
                    {
                        SoundInfo s = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                        s.Icone = "";
                        (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                        (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, s);
                        (PlaylistCollection.ElementAt(curPlaylist)).cur = 0;
                    }
                    curPlay = curPlaylist;
                    SoundInfo curS = (PlaylistCollection.ElementAt(curPlaylist)).SoundCollection.ElementAt(0);
                    if (play)
                        curS.Icone = "/MediaPlayer;component/Images/playing.png";
                    SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlaylist)).cur);
                    SoundCollection.Insert((PlaylistCollection.ElementAt(curPlaylist)).cur, curS);
                    liste.SelectedItem = curS;
                    RoutedEventArgs newEventArgs = new RoutedEventArgs(SelectedEvent);
                    RaiseEvent(newEventArgs);
                }
            }
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
                if (p.Name != "Current list")
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
                SoundCollection.Clear();
                SearchPlaylist(filename);

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

        public class ListViewData
        {
            public ListViewData()
            {
                // default constructor
            }

            public ListViewData(string col1, string col2)
            {
                Col1 = col1;
                Col2 = col2;
            }

            public string Col1 { get; set; }
            public string Col2 { get; set; }
        }
        private void init_Library()
        {
            if (System.IO.File.Exists("Library.xml"))
            {
                if (lib.list_media != null)
                    foreach (Media m in lib.list_media)
                    {
                        if (System.IO.File.Exists(m.Path))
                        {
                            m.media = new MediaElement();
                            m.media.LoadedBehavior = MediaState.Manual;
                            m.media.UnloadedBehavior = MediaState.Manual;
                            m.media.Source = new Uri(m.Path);
                            m.media.MediaOpened += new RoutedEventHandler(MediaOpened);
                            m.media.Pause();
                            add_To_Lib(m);
                        }
                    }
            }
        }

        private Media set_File_Info(String filename, String type)
        {
            Console.WriteLine("Filename = "+ filename);
            Media m = new Media();
            m.Path = filename;
            m.Type = type;
            Console.WriteLine("Path = "+ m.Path);
            tagFile = TagLib.File.Create(filename);

            m.Title = tagFile.Tag.Title;
            if (tagFile.Tag.Title == null)
            {
                m.Title = System.IO.Path.GetFileNameWithoutExtension(filename);
            }
            
            m.media = new MediaElement();
            m.media.LoadedBehavior = MediaState.Manual;
            m.media.UnloadedBehavior = MediaState.Manual;
            m.media.Source = new Uri(filename);
            if (type == "music" || type == "video" || type == "image")
                m.media.MediaOpened += new RoutedEventHandler(MediaOpened);         
            m.media.Pause();
            
            //TimeSpan ts = s.media.NaturalDuration.TimeSpan;
            //if (ts.Seconds < 10 && ts.Minutes < 10)
            //    m.Duration = ts.Hours + ":0" + ts.Minutes + ":0" + ts.Seconds;
            //else if (ts.Seconds < 10 && ts.Minutes >= 10)
            //    m.Duration = ts.Hours + ":" + ts.Minutes + ":0" + ts.Seconds;
            //else if (ts.Seconds >= 10 && ts.Minutes < 10)
            //    m.Duration = ts.Hours + ":0" + ts.Minutes + ":" + ts.Seconds;
            //else if (ts.Seconds >= 10 && ts.Minutes >= 10)
            //    m.Duration = ts.Hours + ":" + ts.Minutes + ":" + ts.Seconds;
            Console.WriteLine("HELLO");
            Console.WriteLine("Title = " + m.Title);
            Console.WriteLine("Duration = "+ m.Duration);
            
            return m;
        }

        private void add_To_Lib(Media m)
        {
            if (m.Type == "video")
                VideoCollection.Add(m);
            else if (m.Type == "music")
                MusicCollection.Add(m);
            else if (m.Type == "image")
                ImageCollection.Add(m);
        }

        private void PlaylistDrop(object sender, DragEventArgs e)
        {
            String[] FileName = (String[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
            foreach (string file in FileName)
            {
                if (System.IO.Path.GetExtension(file) == ".m3u")
                    SearchPlaylist(file);
            }

        }
        private void Musique_Drop(object sender, DragEventArgs e)
        {
            String[] FileName = (String[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
            if (lib.init == false)
                lib.init_Elements();
            foreach (string file in FileName)
            {
                add_To_Lib(lib.add_Media(set_File_Info(file, "music")));
            }
        }

        private void Video_Drop(object sender, DragEventArgs e)
        {
            String[] FileName = (String[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
            if (lib.init == false)
                lib.init_Elements();
            foreach (string file in FileName)
            {
                add_To_Lib(lib.add_Media(set_File_Info(file, "video")));
            }
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            String[] FileName = (String[])e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
            if (lib.init == false)
                lib.init_Elements();
            foreach (string file in FileName)
            {
                add_To_Lib(lib.add_Media(set_File_Info(file, "image")));
            }

        }

        private void liste_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            for (int i = 0; i < ((System.Windows.DataObject)e.Data).GetFileDropList().Count; i++)
            {
                filename = (string)((System.Windows.DataObject)e.Data).GetFileDropList()[i];
                AddSound(filename);
            }
        }

        private void VideoClick(object sender, MouseButtonEventArgs e)
        {

            Media p = (Media)Video.SelectedItem;
            if (p != null)
            {
                AddSound(p.Path);
                SoundInfo s = null;
                foreach (SoundInfo tmp in SoundCollection)
                {
                    if (tmp.FileName == p.Path)
                    {
                        s = tmp;
                        break;
                    }
                }
                if (s != null)
                {
                    if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
                    {
                        SoundInfo p1 = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                        p1.Icone = "";
                        (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                        (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p1);
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
            }
        }
        private void MusicClick(object sender, MouseButtonEventArgs e)
        {
            
            Media p = (Media)Musique.SelectedItem;
            if (p != null)
            {
                AddSound(p.Path);
                SoundInfo s = null;
                foreach (SoundInfo tmp in SoundCollection)
                {
                    if (tmp.FileName == p.Path)
                    {
                        s = tmp;
                        break;
                    }
                }
                if (s != null)
                {
                    if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
                    {
                        SoundInfo p1 = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                        p1.Icone = "";
                        (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                        (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p1);
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
            }
        }
//            Console.WriteLine(p.Path);
        private void ImageClick(object sender, MouseButtonEventArgs e)
        {

            Media p = (Media)Image.SelectedItem;
            if (p != null)
            {
                AddSound(p.Path);
                SoundInfo s = null;
                foreach (SoundInfo tmp in SoundCollection)
                {
                    if (tmp.FileName == p.Path)
                    {
                        s = tmp;
                        break;
                    }
                }
                if (s != null)
                {
                    if ((PlaylistCollection.ElementAt(curPlay)).SoundCollection.Count > 0)
                    {
                        SoundInfo p1 = (PlaylistCollection.ElementAt(curPlay)).SoundCollection.ElementAt((PlaylistCollection.ElementAt(curPlay)).cur);
                        p1.Icone = "";
                        (PlaylistCollection.ElementAt(curPlay)).SoundCollection.RemoveAt((PlaylistCollection.ElementAt(curPlay)).cur);
                        (PlaylistCollection.ElementAt(curPlay)).SoundCollection.Insert((PlaylistCollection.ElementAt(curPlay)).cur, p1);
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
            }
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
        public string Time
        {
            get
            {
                string ret = "";
                double h = 0;
                double m = 0;
                double sec = 0;
                foreach (SoundInfo s in _SoundCollection)
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
                    ret += "--:--";
                    return ret;
                }
                if (h > 0)
                {
                    if (h < 10)
                    {
                        ret += "0";
                    }
                    ret += h + ":";

                }
                if (m < 10)
                {
                    ret += "0";
                }
                ret += m + ":";
                if (sec < 10)
                {
                    ret += "0";
                }
                ret += sec;

                return ret;
            }
        }
        public int id;
        public int cur;
      

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
