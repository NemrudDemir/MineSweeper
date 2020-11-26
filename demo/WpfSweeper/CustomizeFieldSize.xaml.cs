using System.Windows;
using SweeperModel;

namespace WpfSweeper
{
    /// <summary>
    /// Interaction logic for CustomizeField.xaml
    /// </summary>
    public partial class CustomizeFieldSize : Window
    {
        public CustomizeFieldSize(FieldSize currentFieldSize)
        {
            InitializeComponent();
            sldWidth.Minimum = FieldSize.MIN_XY;
            sldWidth.Maximum = FieldSize.MAX_XY;
            sldHeight.Minimum = FieldSize.MIN_XY;
            sldHeight.Maximum = FieldSize.MAX_XY;

            sldWidth.Value = currentFieldSize.X;
            sldHeight.Value = currentFieldSize.Y;
            sldMines.Value = currentFieldSize.MinesTotal;
        }

        private void SldWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblBreite.Content = (int)sldWidth.Value;
            UpdateMaxMines();
        }

        private void SldHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblHoehe.Content = (int)sldHeight.Value;
            UpdateMaxMines();
        }

        private void UpdateMaxMines()
        {
            sldMines.Minimum = FieldSize.GetMinMines();
            var maxMines = FieldSize.GetMaxMines((int)sldWidth.Value, (int)sldHeight.Value);
            if(sldMines.Value > maxMines)
                sldMines.Value = maxMines;
            sldMines.Maximum = maxMines;
        }

        private void SldMines_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblMines.Content = (int)sldMines.Value;
        }

        private void CmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CmdConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(DialogResult == null)
                DialogResult = false;
        }

        public int GetWidth()
        {
            return (int)sldWidth.Value;
        }

        public int GetHeight()
        {
            return (int)sldHeight.Value;
        }

        public int GetMines()
        {
            return (int)sldMines.Value;
        }
    }
}
