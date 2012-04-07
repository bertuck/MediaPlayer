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

    public partial class StyleSubtitle : Window
    {
        MainWindow _ui;

        public StyleSubtitle()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;
        }

        public StyleSubtitle(MainWindow ui)
        {
            _ui = ui;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown test = new NumericUpDown();
            test.Show();
            for (int x = 8; x < 40; x++)
                Size.Items.Add(x);
            for (int x = 0; x <= 10; x++)
                Spacing.Items.Add(x);
            for (int x = 0; x <= 100; x++)
            {
                AngleZ.Items.Add(x);
                ScaleX.Items.Add(x);
                ScaleY.Items.Add(x);
            }
            for (double x = 0; x < 1; x += 0.1)
                Opacity.Items.Add(x);
            PickUpColor.ItemsSource = typeof(System.Windows.Media.Colors).GetProperties();
        }

        private void MoreSynchro_Click(object sender, RoutedEventArgs e)
        {
            char delimiterChars = ' ';
            string[] Tmp = Synchro.Text.Split(delimiterChars);
            _ui.currentTimeSubtitle = Double.Parse(Tmp[0]);
            Console.WriteLine("Time Sub = " + _ui.currentTimeSubtitle);
            _ui.currentTimeSubtitle += 0.1;
            Synchro.Text = _ui.currentTimeSubtitle.ToString() + " s";
        }

        private void LessSynchro_Click(object sender, RoutedEventArgs e)
        {
            char delimiterChars = ' ';
            string[] Tmp = Synchro.Text.Split(delimiterChars);
            _ui.currentTimeSubtitle = Double.Parse(Tmp[0]);
            _ui.currentTimeSubtitle -= 0.1;
            Synchro.Text = _ui.currentTimeSubtitle.ToString() + " s";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Opacity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Opacity.SelectedItem != null)
                _ui.Subtitle.Opacity = double.Parse(Opacity.SelectedItem.ToString());
        }

        private void PickUpColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PickUpColor.SelectedItem != null)
            {
                char delimiterChars = ' ';
                string[] NameColor = PickUpColor.SelectedItem.ToString().Split(delimiterChars);
                var color = System.Drawing.Color.FromName(NameColor[1]);
                _ui.Subtitle.Foreground = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }

        private void comboFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboFonts.SelectedItem != null)
                _ui.Subtitle.FontFamily = new System.Windows.Media.FontFamily(comboFonts.SelectedItem.ToString());
        }

        private void Size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Size.SelectedItem != null)
                _ui.Subtitle.FontSize = int.Parse(Size.SelectedItem.ToString());
        }

    }
}
