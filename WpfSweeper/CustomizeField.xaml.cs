using SweeperModel;
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
    /// Interaction logic for CustomizeField.xaml
    /// </summary>
    public partial class CustomizeField : Window
    {
        public CustomizeField(Field currentField)
        {
            InitializeComponent();
            sldBreite.Minimum = Field.MinX;
            sldBreite.Maximum = Field.MaxX;
            sldHoehe.Minimum = Field.MinY;
            sldHoehe.Maximum = Field.MaxY;

            sldBreite.Value = currentField.X;
            sldHoehe.Value = currentField.Y;
            sldMinen.Value = currentField.MinesTotal;
        }

        private void sldBreite_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblBreite.Content = (int)sldBreite.Value;
            UpdateMaxMines();
        }

        private void sldHoehe_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblHoehe.Content = (int)sldHoehe.Value;
            UpdateMaxMines();
        }

        private void UpdateMaxMines()
        {
            sldMinen.Minimum = Field.GetMinMines((int)sldBreite.Value, (int)sldHoehe.Value);
            sldMinen.Maximum = Field.GetMaxMines((int)sldBreite.Value, (int)sldHoehe.Value);
        }

        private void sldMinen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblMinen.Content = (int)sldMinen.Value;
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void cmdConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == null)
                DialogResult = false;
        }

        public int GetWidth()
        {
            return (int)sldBreite.Value;
        }

        public int GetHeight()
        {
            return (int)sldHoehe.Value;
        }

        public int GetMines()
        {
            return (int)sldMinen.Value;
        }
    }
}
