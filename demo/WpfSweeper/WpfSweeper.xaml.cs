using SweeperModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SweeperModel.Elements;

namespace WpfSweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double CellPixels = 32;
        private const double LineThickness = 1;
        private Field Field { get; set; }
        private DispatcherTimer UpdateTimer { get; }

        public MainWindow()
        {
            InitializeComponent();
            //Timer to update the time of the game
            UpdateTimer = new DispatcherTimer(DispatcherPriority.Normal);
            UpdateTimer.Tick += UpdateTimer_Tick;
            UpdateTimer.Interval = TimeSpan.FromMilliseconds(100);
            UpdateTimer.Start();
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
            var milliseconds = Field?.GetElapsedMilliseconds ?? 0;
            var seconds = (int)(milliseconds / 1000);
            var minutes = seconds / 60;
            seconds %= 60;
            lblTimer.Content = $"{minutes:00}:{seconds:00}"; //Format is mm:ss
        }
        
        /// <summary>
        /// Dynamically generate the menu
        /// </summary>
        private void GenerateMenu()
        {
            //add the predefined fields menu items
            foreach (Field.Standards predefinedField in Enum.GetValues(typeof(Field.Standards))) {
                var mnuItem = new MenuItemNewField(predefinedField) {
                    Header = predefinedField,
                };
                mnuItem.Click += mnuNewPredefinedField_Click;
                mnuNew.Items.Add(mnuItem);
            }
            //add the customize menu item
            var mnuCustomized = new MenuItem() {
                Header = "Custom"
            };
            mnuCustomized.Click += mnuCustomized_Click;
            mnuNew.Items.Add(mnuCustomized);
        }

        private void mnuCustomized_Click(object sender, RoutedEventArgs e)
        {
            var customizeForm = new CustomizeField(Field);
            if (customizeForm.ShowDialog() == true)
                SetField(customizeForm.GetWidth(), customizeForm.GetHeight(), customizeForm.GetMines());
        }

        /// <summary>
        /// New game
        /// </summary>
        /// <param name="field">field</param>
        private void SetField(Field field)
        {
            this.Field = field;
            InitDraw();
        }

        /// <summary>
        /// Initialization of the canvas, all cells on field closed
        /// </summary>
        private void InitDraw()
        {
            cnvField.Children.Clear();
            lblMines.Content = Field.MinesTotal;

            var canvasWidth = Field.X * (CellPixels + LineThickness) + LineThickness;
            var canvasHeight = Field.Y * (CellPixels + LineThickness) + LineThickness;

            Width = canvasWidth + cnvField.Margin.Left + cnvField.Margin.Right + 7 + 7;
            Height = canvasHeight + cnvField.Margin.Top + cnvField.Margin.Bottom + 7 + 30;

            for (var x = 0; x < Field.X; x++) {
                for (var y = 0; y < Field.Y; y++) {
                    var image = new Image {
                        Width = CellPixels,
                        Height = CellPixels,
                        Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/Cell.png", UriKind.Absolute))
                    };
                    cnvField.Children.Add(image);
                    Canvas.SetTop(image, y*(CellPixels + LineThickness));
                    Canvas.SetLeft(image, x*(CellPixels + LineThickness));
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
            if (sender is MenuItemNewField mnuItem)
                SetField(Field.GetStandardsField(mnuItem.FieldType));
        }

        private void cnvField_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetFieldAt(e.GetPosition(cnvField)) is PointI fieldPoint)
                UpdateGame(Field.DoOperation(fieldPoint, Field.Mode.Flag));
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
        private void UpdateGame(IEnumerable<PointI> changedCells)
        {
            var cells = Field.Cells;
            lblMines.Content = Field.MinesLeft;
            foreach (var point in changedCells) {
                var cell = cells[point.X][point.Y];
                var image = new Image {
                    Width = CellPixels,
                    Height = CellPixels,
                };
                switch (cell.Status)
                {
                    case CellStatus.Covered:
                        image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/Cell.png", UriKind.Absolute));
                        break;
                    case CellStatus.Flagged:
                        image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/Flagged.png", UriKind.Absolute));
                        break;
                    case CellStatus.Opened:
                        image.Source = new BitmapImage(new Uri($@"pack://Application:,,,/Ressources/{(int)cell.Value}.png", UriKind.Absolute));
                        break;
                }
                cnvField.Children.Add(image);
                Canvas.SetTop(image, point.Y * (CellPixels + LineThickness));
                Canvas.SetLeft(image, point.X * (CellPixels + LineThickness));
            }
            UpdateStatus();
        }

        /// <summary>
        /// Update canvas, show field after game over
        /// </summary>
        private void UpdateGameOver()
        {
            var cells = Field.Cells;
            for(var x = 0; x<cells.Length; x++) {
                for(var y = 0; y<cells[x].Length; y++) {
                    var cell = cells[x][y];
                    if(cell.Value == CellValue.Mine) {
                        var image = new Image {
                            Width = CellPixels,
                            Height = CellPixels,
                        };
                        switch (cell.Status) {
                            case CellStatus.Covered: //show mine
                                image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/-1.png", UriKind.Absolute));
                                break;
                            case CellStatus.Opened: //the opened mine is highlighted
                                image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/mineRed.png", UriKind.Absolute));
                                break;
                        }
                        cnvField.Children.Add(image);
                        Canvas.SetTop(image, y * (CellPixels + LineThickness));
                        Canvas.SetLeft(image, x * (CellPixels + LineThickness));
                    } else if(cell.Status == CellStatus.Flagged && cell.Value != CellValue.Mine) { //wrong flagged cells
                        var image = new Image {
                            Width = CellPixels,
                            Height = CellPixels,
                            Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/mineX.png", UriKind.Absolute))
                        };
                        cnvField.Children.Add(image);
                        Canvas.SetTop(image, y * (CellPixels + LineThickness));
                        Canvas.SetLeft(image, x * (CellPixels + LineThickness));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the field for the given MousePosition
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        private PointI GetFieldAt(Point mousePosition)
        {
            var divisor = CellPixels + LineThickness;
            var x = (int)(mousePosition.X / divisor);
            var y = (int)(mousePosition.Y / divisor);
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
                if((new GameOver(Field.GetElapsedMilliseconds).ShowDialog()) == true) { //Game over
                    UpdateGameOver();
                } else { //Undo
                    UpdateGame(Field.Undo());
                }
            } else if(Field.IsGameWon) {
                cmdStatus.Content = "B)";
                if((new GameWon(Field.GetElapsedMilliseconds).ShowDialog()) == true) {
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