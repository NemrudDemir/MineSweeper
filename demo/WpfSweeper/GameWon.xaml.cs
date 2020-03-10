using System.Windows;

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
            var minutes = seconds / 60;
            seconds %= 60;
            lblTime.Content = $"{minutes:00}:{seconds:00}"; //format is mm:ss
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
