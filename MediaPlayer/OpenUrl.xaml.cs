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

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        MainWindow _win;
        public Window1(MainWindow win)
        {
            _win = win;
            InitializeComponent();
        }

        private void PlayUrl_Click(object sender, RoutedEventArgs e)
        {
            _win.mediaControl.Source = new Uri(textBox1.Text, UriKind.Absolute);
            _win.play = false;
            _win.Window.Height = 319;
            _win.play_Click(this, e);
            Close();
        }
    }
}
