using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        MainWindow _win;
        bool SubLoaded = false;
        public Window1(MainWindow win)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;
            _win = win;
            InitializeComponent();
        }

        private void PlayUrl_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text.Length >= 1)
            {
                _win.mediaControl.Source = new Uri(textBox1.Text, UriKind.Absolute);
                _win.playlist.AddUrl(textBox1.Text);
                _win.play = false;
                _win.Window.Height = 319;
                _win.play_Click(this, e);
                
                Close();
            }
        }

        public void loadSubtitle_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog os = new OpenFileDialog();
            os.AddExtension = true;
            os.DefaultExt = "*.*";
            os.Filter = "Media (*.*)|*.*";
            os.ShowDialog();
            if (os.FileName != "")
            {
                SubLoaded = true;
                _win.loadSubtitle(os.FileName, true);
                urlSubtitle.Text = os.FileName;
            }
        }
    }
}
