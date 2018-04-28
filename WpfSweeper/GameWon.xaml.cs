using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfSweeper
{
    /// <summary>
    /// Interaction logic for GameWon.xaml
    /// </summary>
    public partial class GameWon : Window
    {
        public GameWon(long milliseconds)
        {
            InitializeComponent();
            var seconds = milliseconds / 1000;
            milliseconds %= 1000;
            var minutes = seconds / 60;
            seconds %= 60;
            lblTime.Content = $"{minutes.ToString("00")}:{seconds.ToString("00")}"; //format is mm:ss
        }

        private void cmdShowField_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void cmdNewGame_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
