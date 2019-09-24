using System.Windows;

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
            var minutes = seconds / 60;
            seconds %= 60;
            lblTime.Content = $"{minutes:00}:{seconds:00}"; //format is mm:ss
        }
        
        private void cmdGameOver_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void cmdUndo_Click(object sender, RoutedEventArgs e)
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
