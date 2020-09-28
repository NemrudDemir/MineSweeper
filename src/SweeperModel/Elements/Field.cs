using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace SweeperModel.Elements
{
    /// <summary>
    /// Minesweeper field
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Minimum Width/Height
        /// </summary>
        public const int MIN_XY = 9;

        /// <summary>
        /// Maximum Width/Height
        /// </summary>
        public const int MAX_XY = 30;

        private const int MIN_MINES = 10;

        /// <summary>
        /// Gets the minimum mines 
        /// </summary>
        /// <returns></returns>
        public static int GetMinMines() => MIN_MINES;

        /// <summary>
        /// Gets the maximum mines
        /// </summary>
        /// <param name="x">width of field</param>
        /// <param name="y">height of field</param>
        /// <returns></returns>
        public static int GetMaxMines(int x, int y)
        {
            return x * y - (x + y - 1);
        }

        private readonly Stopwatch _stopWatch = new Stopwatch();
        /// <summary>
        /// Get Elapsed Milliseconds
        /// </summary>
        public long GetElapsedMilliseconds => _stopWatch.ElapsedMilliseconds;

        public int X {
            get;
        }

        public int Y {
            get;
        }

        public int MinesTotal {
            get;
        }

        /// <summary>
        /// Indicator for how many mines left on the field
        /// </summary>
        public int MinesLeft {
            get; private set;
        }

        /// <summary>
        /// Indicator for how many non-mines cells opened on the field
        /// </summary>
        private int NonMinesOpened {
            get; set;
        }

        public Cell[][] Cells { //TODO Cells[,] ?!
            get; set;
        }
        private bool _isFieldInitialized;

        private PointI _lastOpenedPoint; //is required for undo operation
        public GameStatus GameStatus {
            get; private set;
        }

        private UserSettings _userSettings;
        public UserSettings UserSettings {
            get => _userSettings ?? UserSettings.Default;
            set => _userSettings = value;
        }

        /// <summary>
        /// Initializes a field
        /// </summary>
        /// <param name="x">width</param>
        /// <param name="y">heigh</param>
        /// <param name="mines">number of mines</param>
        public Field(int x, int y, int mines) : this(x, y, mines, false) { }

        /// <summary>
        /// Initializes a field
        /// </summary>
        /// <param name="x">width</param>
        /// <param name="y">heigh</param>
        /// <param name="mines">number of mines</param>
        /// <param name="reversed">swaps width and height</param>
        public Field(int x, int y, int mines, bool reversed)
        {
            X = x;
            Y = y;
            if (reversed) {
                Y = x;
                X = y;
            }
            MinesTotal = MinesLeft = mines;

            if(mines > GetMaxMines(x, y) || mines < GetMinMines() || x > MAX_XY || y > MAX_XY || x < MIN_XY ||y < MIN_XY)
                throw new Exception($"The given field is invalid!{Environment.NewLine}");

            InitializeCells();
        }

        private void InitializeCells()
        {
            Cells = new Cell[X][];
            for(var row = 0; row < X; row++) {
                Cells[row] = new Cell[Y];
                for(var column = 0; column < Y; column++) {
                    Cells[row][column] = new Cell();
                }
            }
        }

        /// <summary>
        /// Setting the mines on the field
        /// </summary>
        /// <param name="hitPoint">point hit</param>
        public void InitializeCellValues(PointI hitPoint)
        {
            var potentialMineIndices = new List<PointI>();
            for (var row = 0; row < X; row++) {
                for (var column = 0; column < Y; column++) {
                    potentialMineIndices.Add(new PointI(row, column));
                }
            }
            
            var protectedCellIndices = GetNearbyCellPoints(hitPoint, true).
                Select(GenerateNumberFromPoint).OrderByDescending(number => number);
            foreach(var index in protectedCellIndices)
                potentialMineIndices.RemoveAt(index);

            var minesToSet = MinesTotal;
            var random = new Random();
            while(minesToSet > 0) { //Create indices for mines
                var rngIndex = random.Next(potentialMineIndices.Count);
                SetMine(potentialMineIndices[rngIndex]);
                potentialMineIndices.RemoveAt(rngIndex);
                minesToSet--;
            }
            _isFieldInitialized = true;
            _stopWatch.Start();
        }

        /// <summary>
        /// Set cell as mine
        /// </summary>
        /// <param name="point">Point of the mine</param>
        private void SetMine(PointI point)
        {
            Cells[point.X][point.Y].SetAsMine();
            UpdateNearbyCells(point);
        }

        public IEnumerable<PointI> GetNearbyCellPoints(PointI midPoint)
        {
            return GetNearbyCellPoints(midPoint, false);
        }

        /// <summary>
        /// Gets all the points nearby the given point
        /// </summary>
        /// <param name="midPoint">middle Point</param>
        /// <param name="withOwnCell">Adds the given point to the collection</param>
        /// <returns></returns>
        public IEnumerable<PointI> GetNearbyCellPoints(PointI midPoint, bool withOwnCell)
        {
            var x = midPoint.X;
            var y = midPoint.Y;
            var nearbyCells = new List<PointI> {
                GetCellPoint(x - 1, y - 1), //topleft
                GetCellPoint(x, y - 1), //top
                GetCellPoint(x + 1, y - 1), //topright
                GetCellPoint(x - 1, y), //left
                GetCellPoint(x + 1, y), //right
                GetCellPoint(x - 1, y + 1), //bottom left
                GetCellPoint(x, y + 1), //bottom
                GetCellPoint(x + 1, y + 1) //bottom right
            };
            if(withOwnCell)
                nearbyCells.Add(midPoint); //own
            return nearbyCells.Where(point => point != null);
        }

        /// <summary>
        /// Gets all the cells nearby the given point
        /// </summary>
        /// <param name="point">middle Point</param>
        /// <returns></returns>
        private IEnumerable<Cell> GetNearbyCells(PointI point)
        {
            return GetNearbyCellPoints(point).Select(cPoint => Cells[cPoint.X][cPoint.Y]);
        }

        /// <summary>
        /// Gets the Point if it is valid, otherwise null
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns></returns>
        private PointI GetCellPoint(int x, int y)
        {
            if(x >= 0 && x < X && y >= 0 && y < Y)
                return new PointI(x, y);
            return null;
        }

        /// <summary>
        /// Update the values of the nearby cells
        /// </summary>
        /// <param name="point">point of cell</param>
        private void UpdateNearbyCells(PointI point)
        {
            foreach(var nearbyCell in GetNearbyCells(point))
                nearbyCell.AddNearbyMine();
        }

        /// <summary>
        /// Generates an one dimensional number from a given point
        /// </summary>
        /// <param name="point">2 dimensional Point</param>
        /// <returns></returns>
        private int GenerateNumberFromPoint(PointI point)
        {
            return Y * point.X + point.Y;
        }

        /// <summary>
        /// Do an operation to the game
        /// </summary>
        /// <param name="point">cell of the operation</param>
        /// <param name="mode">the mode of the operation</param>
        /// <returns>the manipulated points</returns>
        public List<PointI> DoOperation(PointI point, Mode mode)
        {
            if(GameStatus != GameStatus.Open)
                return new List<PointI>();
            if(!_isFieldInitialized)
                InitializeCellValues(point);

            switch(mode) {
                case Mode.Open:
                    return OpenCell(point);
                case Mode.Flag:
                    ToggleFlag(point);
                    return new List<PointI> { point };
                case Mode.OpenNearby:
                    return OpenNearbyCellsOrSelf(point);
            }
            return new List<PointI>();
        }

        /// <summary>
        /// Opens the given cell
        /// </summary>
        /// <param name="point">Point for the operation</param>
        /// <param name="changedCells">the manipulated points</param>
        private List<PointI> OpenCell(PointI point)
        {
            var cell = Cells[point.X][point.Y];
            if(GameStatus != GameStatus.Open || cell.Status != CellStatus.Covered) //only covered cells can be opened
                return new List<PointI>();

            var changedCells = new List<PointI>();
            _lastOpenedPoint = point;
            cell.Status = CellStatus.Opened;
            changedCells.Add(point);
            NonMinesOpened++;
            if(cell.Value == CellValue.Mine)
            { //game over
                _stopWatch.Stop();
                GameStatus = GameStatus.Lost;
            }
            else if(NonMinesOpened + MinesTotal == X * Y)
            { //game won
                _stopWatch.Stop();
                GameStatus = GameStatus.Won;
            }
            else if(cell.Value == CellValue.Empty)
            { //if there are no nearby mines, open all nearby cells automatically
                foreach(var nearbyCellPoint in GetNearbyCellPoints(point))
                {
                    var emptyChangedCells = OpenCell(nearbyCellPoint);
                    changedCells.AddRange(emptyChangedCells);
                }
            }
            return changedCells;
        }

        /// <summary>
        /// Toggles flag on the given cell
        /// </summary>
        /// <param name="point">Point for the operation</param>
        private void ToggleFlag(PointI point)
        {
            var cell = Cells[point.X][point.Y];
            switch(cell.Status) {
                case CellStatus.Covered:
                    cell.Status = CellStatus.Flagged;
                    MinesLeft--;
                    break;
                case CellStatus.Flagged:
                    cell.Status = CellStatus.Covered;
                    MinesLeft++;
                    break;
            }
        }

        /// <summary>
        /// Opens all nearby cells if the nearby flags are equal to the value of the cell
        /// </summary>
        /// <param name="point">Point for the operation</param>
        /// <param name="changedCells">the manipulated points</param>
        private List<PointI> OpenNearbyCellsOrSelf(PointI point)
        {
            var cell = Cells[point.X][point.Y];
            if(cell.Status == CellStatus.Covered) {
                return OpenCell(point); //if the cell is still covered, open it
            } else if(cell.Status == CellStatus.Opened) {
                return OpenNearbyCells(point);
            }
            return new List<PointI>();
        }

        private List<PointI> OpenNearbyCells(PointI point)
        {
            var changedCells = new List<PointI>();
            var cell = Cells[point.X][point.Y];
            var nearbyCellPoints = GetNearbyCellPoints(point).ToList();
            var flagCounter = nearbyCellPoints.Count(nearbyCellPoint =>
                Cells[nearbyCellPoint.X][nearbyCellPoint.Y].Status == CellStatus.Flagged);
            if(flagCounter == (int)cell.Value)
            { //if the flag count on nearby cells equal to the value of mines nearby open nearby cells
                foreach(var nearbyCellPoint in nearbyCellPoints) {
                    var tempChangedCells = OpenCell(nearbyCellPoint);
                    changedCells.AddRange(tempChangedCells);
                }
            }

            if(UserSettings.DoOpenNearbyRecursive) {
                var recursiveChangedCells = new List<PointI>();
                foreach(var changedCell in changedCells) {
                    var tempChangedCells = OpenNearbyCells(changedCell);
                    recursiveChangedCells.AddRange(tempChangedCells);
                }
                changedCells.AddRange(recursiveChangedCells);
            }
            return changedCells;
        }

        /// <summary>
        /// On game over, undo the last operation, so that the game keeps going
        /// </summary>
        /// <returns>the manipulated cell</returns>
        public PointI Undo()
        {
            if(GameStatus != GameStatus.Lost)
                return null;

            GameStatus = GameStatus.Open;
            _stopWatch.Start();
            Cells[_lastOpenedPoint.X][_lastOpenedPoint.Y].Status = CellStatus.Covered;
            NonMinesOpened--;
            return _lastOpenedPoint;
        }

        /// <summary>
        /// Standard fields
        /// </summary>
        public enum Standards
        {
            [Description("Beginner")]
            Beginner,
            [Description("Intermediate")]
            Intermediate,
            [Description("Expert")]
            Expert
        }

        /// <summary>
        /// Operation modes
        /// </summary>
        public enum Mode
        {
            Open,
            Flag,
            OpenNearby
        }

        /// <summary>
        /// Gets the Field for a standard-field
        /// </summary>
        /// <param name="predefinedField">Standard field</param>
        /// <returns>returns a field with certain options</returns>
        public static Field GetStandardsField(Standards predefinedField) //https://www.bernhard-gaul.de/spiele/minesweeper/minesweeper-spielregel.html
        {
            switch(predefinedField) {
                case Standards.Beginner:
                    return new Field(9, 9, 10);
                case Standards.Intermediate:
                    return new Field(16, 16, 40);
                case Standards.Expert:
                    return new Field(30, 16, 99);
            }

            return null;
        }
    }
}