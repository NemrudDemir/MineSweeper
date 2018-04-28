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
    /// Interaction logic for GameOver.xaml
    /// </summary>
    public partial class GameOver : Window
    {
        public GameOver(long milliseconds)
        {
            InitializeComponent();
            var seconds = milliseconds / 1000;
            milliseconds %= 1000;
            var minutes = seconds / 60;
            seconds %= 60;
            lblTime.Content = $"{minutes.ToString("00")}:{seconds.ToString("00")}"; //format is mm:ss
        }
        
        private void cmdGameOver_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void cmdRedo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(DialogResult == null)
                DialogResult = true;
        }
    }
}
