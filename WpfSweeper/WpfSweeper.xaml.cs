using SweeperModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfSweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double _CellPixels = 32;
        const double _LineThickness = 1;
        Field Field;

        private DispatcherTimer updateTimer;

        public MainWindow()
        {
            InitializeComponent();
            //Timer to update the time of the game
            updateTimer = new DispatcherTimer(DispatcherPriority.Normal);
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Interval = TimeSpan.FromMilliseconds(100);
            updateTimer.Start();
            GenerateMenu(); //dynamically generating the menu
            SetField(Field.GetStandardsField(Field.Standards.Beginner)); //Initialize field
        }

        /// <summary>
        /// Update the game timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var milliseconds = Field?.GetTime;
            int seconds = (int)(milliseconds / 1000);
            milliseconds %= 1000;
            int minutes = seconds / 60;
            seconds %= 60;
            lblTimer.Content = $"{minutes.ToString("00")}:{seconds.ToString("00")}"; //Format is mm:ss
        }
        
        /// <summary>
        /// Dynamically generate the menu
        /// </summary>
        private void GenerateMenu()
        {
            //add the predefined fields menu items
            foreach (Field.Standards predefinedField in Enum.GetValues(typeof(Field.Standards))) {
                MenuItemNewField mnuItem = new MenuItemNewField(predefinedField) {
                    Header = predefinedField.ToDescription(),
                };
                mnuItem.Click += mnuNewPredefinedField_Click;
                mnuNew.Items.Add(mnuItem);
            }
            //add the customize menu item
            MenuItem mnuCustomized = new MenuItem() {
                Header = "Benutzerdefiniert"
            };
            mnuCustomized.Click += MnuCustomized_Click;
            mnuNew.Items.Add(mnuCustomized);
        }
        
        private void MnuCustomized_Click(object sender, RoutedEventArgs e)
        {
            var customizeForm = new CustomizeField(Field);
            if(customizeForm.ShowDialog() == true) {
                SetField(customizeForm.GetWidth(), customizeForm.GetHeight(), customizeForm.GetMines());
            }
        }

        /// <summary>
        /// New game
        /// </summary>
        /// <param name="field">field</param>
        private void SetField(Field field)
        {
            Field = field;
            InitDraw();
        }

        /// <summary>
        /// Initialization of the canvas, all cells on field closed
        /// </summary>
        private void InitDraw()
        {
            cnvField.Children.Clear();
            lblMines.Content = Field.MinesTotal;

            var canvasWidth = Field.X * (_CellPixels + _LineThickness) + _LineThickness;
            var canvasHeight = Field.Y * (_CellPixels + _LineThickness) + _LineThickness;

            this.Width = canvasWidth + cnvField.Margin.Left + cnvField.Margin.Right + 7 + 7;
            this.Height = canvasHeight + cnvField.Margin.Top + cnvField.Margin.Bottom + 7 + 30;

            for (int x = 0; x < Field.X; x++) {
                for (int y = 0; y < Field.Y; y++) {
                    Image image = new Image {
                        Width = _CellPixels,
                        Height = _CellPixels,
                        Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/Cell.png", UriKind.Absolute))
                    };
                    cnvField.Children.Add(image);
                    Canvas.SetTop(image, y*(_CellPixels + _LineThickness));
                    Canvas.SetLeft(image, x*(_CellPixels + _LineThickness));
                }
            }

            UpdateStatus();
        }

        /// <summary>
        /// New game field
        /// </summary>
        /// <param name="x">width</param>
        /// <param name="y">height</param>
        /// <param name="mines">mines</param>
        private void SetField(int x, int y, int mines)
        {
            SetField(new Field(x, y, mines));
        }

        private void mnuNewPredefinedField_Click(object sender, RoutedEventArgs e)
        {
            MenuItemNewField mnuItem = sender as MenuItemNewField;
            SetField(Field.GetStandardsField(mnuItem.FieldType));
        }

        private void cnvField_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetFieldAt(e.GetPosition(cnvField)) is PointI fieldPoint) {
                UpdateGame(Field.DoOperation(fieldPoint, Field.Mode.Flag));
            }
        }

        private void cnvField_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2) {
                if (GetFieldAt(e.GetPosition(cnvField)) is PointI fieldPoint) {
                    UpdateGame(Field.DoOperation(fieldPoint, Field.Mode.OpenNearby));
                }
            } else {
                cmdStatus.Content = ":O";
            }
            e.Handled = true;
        }

        /// <summary>
        /// Updates the canvas
        /// </summary>
        /// <param name="changedCells">cells which will be replaced</param>
        private void UpdateGame(List<PointI> changedCells)
        {
            var cells = Field.Cells;
            lblMines.Content = Field.MinesLeft;
            foreach (var point in changedCells) {
                Cell cell = cells[point.X][point.Y];
                Image image = new Image {
                    Width = _CellPixels,
                    Height = _CellPixels,
                };
                if (cell.Status == CellStatus.Covered)
                    image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/Cell.png", UriKind.Absolute));
                else if (cell.Status == CellStatus.Flagged)
                    image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/Flagged.png", UriKind.Absolute));
                else if (cell.Status == CellStatus.Opened) {
                    image.Source = new BitmapImage(new Uri($@"pack://Application:,,,/Ressources/{(int)cell.Value}.png", UriKind.Absolute));
                }
                cnvField.Children.Add(image);
                Canvas.SetTop(image, point.Y * (_CellPixels + _LineThickness));
                Canvas.SetLeft(image, point.X * (_CellPixels + _LineThickness));
            }
            UpdateStatus();
        }

        /// <summary>
        /// Update canvas, show field after game over
        /// </summary>
        private void UpdateGameOver()
        {
            var cells = Field.Cells;
            for(int x = 0; x<cells.Length; x++) {
                for(int y = 0; y<cells[x].Length; y++) {
                    Cell cell = cells[x][y];
                    if(cell.Value == CellValue.Mine) {
                        Image image = new Image {
                            Width = _CellPixels,
                            Height = _CellPixels,
                        };
                        switch (cell.Status) {
                            case CellStatus.Covered: //show mine
                                image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/-1.png", UriKind.Absolute));
                                break;
                            case CellStatus.Opened: //the opened mine is highlighted
                                image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/mineRed.png", UriKind.Absolute));
                                break;
                            default:
                                break;
                        }
                        cnvField.Children.Add(image);
                        Canvas.SetTop(image, y * (_CellPixels + _LineThickness));
                        Canvas.SetLeft(image, x * (_CellPixels + _LineThickness));
                    } else if(cell.Status == CellStatus.Flagged && cell.Value != CellValue.Mine) { //wrong flagged cells
                        Image image = new Image {
                            Width = _CellPixels,
                            Height = _CellPixels,
                            Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/mineX.png", UriKind.Absolute))
                        };
                        cnvField.Children.Add(image);
                        Canvas.SetTop(image, y * (_CellPixels + _LineThickness));
                        Canvas.SetLeft(image, x * (_CellPixels + _LineThickness));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the field for the given MousePosition
        /// </summary>
        /// <param name="MousePosition"></param>
        /// <returns></returns>
        private PointI GetFieldAt(Point MousePosition)
        {
            var divisor = _CellPixels + _LineThickness;
            var x = (int)(MousePosition.X / divisor);
            var y = (int)(MousePosition.Y / divisor);
            return new PointI(x, y);
        }

        private void cnvField_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetFieldAt(e.GetPosition(cnvField)) is PointI fieldPoint) {
                UpdateGame(Field.DoOperation(fieldPoint, Field.Mode.Open));
            }
        }

        /// <summary>
        /// Updates game status
        /// </summary>
        private void UpdateStatus()
        {
            if(Field.IsGameOver) {
                cmdStatus.Content = ":(";
                if((new GameOver(Field.GetTime).ShowDialog()) == true) { //Game over
                    UpdateGameOver();
                } else { //Redo
                    UpdateGame(Field.Redo());
                }
            } else if(Field.IsGameWon) {
                cmdStatus.Content = "B)";
                if((new GameWon(Field.GetTime).ShowDialog()) == true) {
                    SetField(Field.X, Field.Y, Field.MinesTotal);
                }
            } else {
                cmdStatus.Content = ":)";
            }
        }

        private void cmdStatus_Click(object sender, RoutedEventArgs e)
        {
            SetField(Field.X, Field.Y, Field.MinesTotal);
        }
    }
}
