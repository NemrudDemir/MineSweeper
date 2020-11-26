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
        private const double CELL_PIXELS = 32;
        private const double LINE_THICKNESS = 1;
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
            SetField(); //Initialize field
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
            foreach(var predefinedFieldSize in FieldSize.Standards)
            {
                var mnuItem = new MenuItemNewField(predefinedFieldSize);
                mnuItem.Click += mnuNewPredefinedField_Click;
                mnuNew.Items.Add(mnuItem);
            }
            //add the customize menu item
            var mnuCustomized = new MenuItem()
            {
                Header = "Custom"
            };
            mnuCustomized.Click += mnuCustomized_Click;
            mnuNew.Items.Add(mnuCustomized);
        }

        private void mnuCustomized_Click(object sender, RoutedEventArgs e)
        {
            var customizeForm = new CustomizeFieldSize(Field.Size);
            if(customizeForm.ShowDialog() == true)
                SetField(new FieldSize(customizeForm.GetWidth(), customizeForm.GetHeight(), customizeForm.GetMines()));
        }

        private void SetField()
        {
            SetField(FieldSize.Beginner);
        }

        private void SetField(FieldSize size)
        {
            SetField(new Field(size));
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
            lblMines.Content = Field.MinesLeft;

            var canvasWidth = Field.Size.X * (CELL_PIXELS + LINE_THICKNESS) + LINE_THICKNESS;
            var canvasHeight = Field.Size.Y * (CELL_PIXELS + LINE_THICKNESS) + LINE_THICKNESS;

            Width = canvasWidth + cnvField.Margin.Left + cnvField.Margin.Right + 7 + 7;
            Height = canvasHeight + cnvField.Margin.Top + cnvField.Margin.Bottom + 7 + 30;

            for(var x = 0; x < Field.Size.X; x++)
            {
                for(var y = 0; y < Field.Size.Y; y++)
                {
                    var image = new Image
                    {
                        Width = CELL_PIXELS,
                        Height = CELL_PIXELS,
                        Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/Cell.png", UriKind.Absolute))
                    };
                    cnvField.Children.Add(image);
                    Canvas.SetTop(image, y * (CELL_PIXELS + LINE_THICKNESS));
                    Canvas.SetLeft(image, x * (CELL_PIXELS + LINE_THICKNESS));
                }
            }
            UpdateStatus();
        }

        private void mnuNewPredefinedField_Click(object sender, RoutedEventArgs e)
        {
            if(sender is MenuItemNewField mnuItem)
                SetField(mnuItem.FieldSize);
        }

        private void cnvField_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(GetFieldAt(e.GetPosition(cnvField)) is PointI fieldPoint)
                UpdateGame(Field.DoOperation(fieldPoint, Field.Mode.Flag));
        }

        private void cnvField_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if(GetFieldAt(e.GetPosition(cnvField)) is PointI fieldPoint)
                {
                    UpdateGame(Field.DoOperation(fieldPoint, Field.Mode.OpenNearby));
                }
            }
            else
            {
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
            foreach(var point in changedCells)
            {
                var cell = cells[point.X, point.Y];
                var image = new Image
                {
                    Width = CELL_PIXELS,
                    Height = CELL_PIXELS,
                };
                switch(cell.Status)
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
                Canvas.SetTop(image, point.Y * (CELL_PIXELS + LINE_THICKNESS));
                Canvas.SetLeft(image, point.X * (CELL_PIXELS + LINE_THICKNESS));
            }
            UpdateStatus();
        }

        private void UpdateGame(PointI changedCell)
        {
            var changedCells = new List<PointI>();
            if(changedCell != null)
            {
                changedCells.Add(changedCell);
            }
            UpdateGame(changedCells);
        }

        /// <summary>
        /// Update canvas, show field after game over
        /// </summary>
        private void UpdateGameOver()
        {
            var cells = Field.Cells;
            for(var x = 0; x < cells.GetLength(0); x++)
            {
                for(var y = 0; y < cells.GetLength(1); y++)
                {
                    var cell = cells[x, y];
                    if(cell.Value == CellValue.Mine)
                    {
                        var image = new Image
                        {
                            Width = CELL_PIXELS,
                            Height = CELL_PIXELS,
                        };
                        switch(cell.Status)
                        {
                            case CellStatus.Covered: //show mine
                                image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/-1.png", UriKind.Absolute));
                                break;
                            case CellStatus.Opened: //the opened mine is highlighted
                                image.Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/mineRed.png", UriKind.Absolute));
                                break;
                        }
                        cnvField.Children.Add(image);
                        Canvas.SetTop(image, y * (CELL_PIXELS + LINE_THICKNESS));
                        Canvas.SetLeft(image, x * (CELL_PIXELS + LINE_THICKNESS));
                    }
                    else if(cell.Status == CellStatus.Flagged && cell.Value != CellValue.Mine)
                    { //wrong flagged cells
                        var image = new Image
                        {
                            Width = CELL_PIXELS,
                            Height = CELL_PIXELS,
                            Source = new BitmapImage(new Uri(@"pack://Application:,,,/Ressources/mineX.png", UriKind.Absolute))
                        };
                        cnvField.Children.Add(image);
                        Canvas.SetTop(image, y * (CELL_PIXELS + LINE_THICKNESS));
                        Canvas.SetLeft(image, x * (CELL_PIXELS + LINE_THICKNESS));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the field for the given MousePosition
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        private static PointI GetFieldAt(Point mousePosition)
        {
            const double divisor = CELL_PIXELS + LINE_THICKNESS;
            var x = (int)(mousePosition.X / divisor);
            var y = (int)(mousePosition.Y / divisor);
            return new PointI(x, y);
        }

        private void cnvField_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(GetFieldAt(e.GetPosition(cnvField)) is PointI fieldPoint)
            {
                UpdateGame(Field.DoOperation(fieldPoint, Field.Mode.Open));
            }
        }

        /// <summary>
        /// Updates game status
        /// </summary>
        private void UpdateStatus()
        {
            if(Field.GameStatus == GameStatus.Lost)
            {
                cmdStatus.Content = ":(";
                if(new GameOver(Field.GetElapsedMilliseconds).ShowDialog() == true)
                { //Game over
                    UpdateGameOver();
                }
                else
                { //Undo
                    UpdateGame(Field.Undo());
                }
            }
            else if(Field.GameStatus == GameStatus.Won)
            {
                cmdStatus.Content = "B)";
                if((new GameWon(Field.GetElapsedMilliseconds).ShowDialog()) == true)
                {
                    SetField(Field.Size);
                }
            }
            else
            {
                cmdStatus.Content = ":)";
            }
        }

        private void cmdStatus_Click(object sender, RoutedEventArgs e)
        {
            SetField(Field.Size);
        }
    }
}