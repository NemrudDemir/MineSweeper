using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SweeperModel
{
    /// <summary>
    /// Minesweeper field
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Minimum Width
        /// </summary>
        public static int MinX => 9;

        /// <summary>
        /// Minimum Height
        /// </summary>
        public static int MinY => 9;

        /// <summary>
        /// Maximum Width
        /// </summary>
        public static int MaxX => 30;

        /// <summary>
        /// Maximum Height
        /// </summary>
        public static int MaxY => 24;

        /// <summary>
        /// Gets the minimum mines 
        /// </summary>
        /// <param name="x">width of field</param>
        /// <param name="y">height of field</param>
        /// <returns></returns>
        public static int GetMinMines(int x, int y)
        {
            return 10;
        }

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
        public int NonMinesOpened {
            get; private set;
        }

        public Cell[][] Cells { //TODO Cells[,] ?!
            get; set;
        }
        private bool _isFieldInitialized;

        private PointI _lastOpenedPoint; //is required for undo operation
        public bool IsGameOver {
            get; private set;
        }

        public bool IsGameWon {
            get; private set;
        }

        /// <summary>
        /// Initializes a field
        /// </summary>
        /// <param name="x">width</param>
        /// <param name="y">heigh</param>
        /// <param name="mines">number of mines</param>
        /// <param name="reversed">swaps width and height</param>
        public Field(int x, int y, int mines, bool reversed = false)
        {
            if(reversed) {
                Y = x;
                X = y;
            } else {
                X = x;
                Y = y;
            }
            MinesTotal = MinesLeft = mines;

            if (mines > GetMaxMines(x, y))
                throw new Exception("There are too many mines for the fieldsize!");

            //Initialize cells
            Cells = new Cell[this.X][];
            for (var row = 0; row < this.X; row++) {
                Cells[row] = new Cell[this.Y];
                for (var column = 0; column < this.Y; column++)
                    Cells[row][column] = new Cell();
            }
        }

        /// <summary>
        /// Setting the mines on the field
        /// </summary>
        /// <param name="hitPoint">point hit</param>
        public void SetMines(PointI hitPoint)
        {
            var minesToSet = MinesTotal;
            var potentialMineIndices = new List<PointI>();
            for (var y = 0; y < Y; y++)
                for (var x = 0; x < X; x++)
                    potentialMineIndices.Add(new PointI(x, y));

            int[] protectedCellIndices = GetNearbyCellPoints(hitPoint, true).
                Select(GenerateNumberFromPoint).OrderByDescending(number => number).ToArray(); //TODO: doesnt work right?!
            foreach(var index in protectedCellIndices)
                potentialMineIndices.RemoveAt(index);

            var random = new Random();
            while (minesToSet > 0) { //Create indices for mines
                int rndIndex = random.Next(potentialMineIndices.Count);
                SetMine(potentialMineIndices[rndIndex]);
                potentialMineIndices.RemoveAt(rndIndex);
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

        /// <summary>
        /// Gets all the points nearby the given point
        /// </summary>
        /// <param name="midPoint">middle Point</param>
        /// <param name="withOwnCell">Adds the given point to the collection</param>
        /// <returns></returns>
        private PointI[] GetNearbyCellPoints(PointI midPoint, bool withOwnCell=false)
        {
            var x = midPoint.X;
            var y = midPoint.Y;
            var nearbyCells = new List<PointI>() {
                GetCellPoint(x - 1, y - 1), //topleft
                GetCellPoint(x, y - 1), //top
                GetCellPoint(x + 1, y - 1), //topright
                GetCellPoint(x - 1, y), //left
                GetCellPoint(x + 1, y), //right
                GetCellPoint(x - 1, y + 1), //bottom left
                GetCellPoint(x, y + 1), //bottom
                GetCellPoint(x + 1, y + 1) //bottom right
            };
            if (withOwnCell)
                nearbyCells.Add(midPoint); //own
            return nearbyCells.Where(point => point != null).ToArray();
        }

        /// <summary>
        /// Gets all the cells nearby the given point
        /// </summary>
        /// <param name="point">middle Point</param>
        /// <param name="withOwnCell">Adds the given point-cell to the collection</param>
        /// <returns></returns>
        private IEnumerable<Cell> GetNearbyCells(PointI point, bool withOwnCell=false)
        {
            return GetNearbyCellPoints(point, withOwnCell).Select(cPoint => Cells[cPoint.X][cPoint.Y]);
        }

        /// <summary>
        /// Gets the Point if it is valid, otherwise null
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns></returns>
        private PointI GetCellPoint(int x, int y)
        {
            if (x >= 0 && x < X && y >= 0 && y < Y)
                return new PointI(x, y);
            return null;
        }

        /// <summary>
        /// Update the values of the nearby cells
        /// </summary>
        /// <param name="point">point of cell</param>
        private void UpdateNearbyCells(PointI point)
        {
            foreach (var nearbyCell in GetNearbyCells(point))
                nearbyCell.AddNearbyMine();
        }
        
        /// <summary>
        /// Generates an one dimensional number from a given point
        /// </summary>
        /// <param name="point">2 dimensional Point</param>
        /// <returns></returns>
        private int GenerateNumberFromPoint(PointI point)
        {
            return X * point.Y + point.X;
        }

        /// <summary>
        /// Generates a two dimensional point from a given number
        /// </summary>
        /// <param name="value">number</param>
        /// <returns></returns>
        private PointI GeneratePointFromNumber(int value)
        {
            return new PointI(value % X, value / X);
        }

        /// <summary>
        /// Do an operation to the game
        /// </summary>
        /// <param name="point">cell of the operation</param>
        /// <param name="mode">the mode of the operation</param>
        /// <returns>the manipulated points</returns>
        public List<PointI> DoOperation(PointI point, Mode mode)
        {
            if (IsGameOver || IsGameWon)
                return new List<PointI>();
            if (!_isFieldInitialized)
                SetMines(point);

            var changedCells = new List<PointI>();
            switch (mode) {
                case Mode.Open:
                    OpenCell(point, ref changedCells);
                    break;
                case Mode.Flag:
                    ToggleFlag(point);
                    changedCells.Add(point);
                    break;
                case Mode.OpenNearby:
                    OpenNearbyCells(point, ref changedCells);
                    break;
            }
            return changedCells;
        }

        /// <summary>
        /// Opens the given cell
        /// </summary>
        /// <param name="point">Point for the operation</param>
        /// <param name="changedCells">the manipulated points</param>
        private void OpenCell(PointI point, ref List<PointI> changedCells)
        {
            Cell cell = Cells[point.X][point.Y];
            if(cell.Status == CellStatus.Covered && !IsGameOver) { //only covered cells can be opened
                _lastOpenedPoint = point;
                cell.Status = CellStatus.Opened;
                changedCells.Add(point);
                NonMinesOpened++;
                if (cell.Value == CellValue.Mine) { //game over
                    _stopWatch.Stop();
                    IsGameOver = true;
                } else if (NonMinesOpened + MinesTotal == X*Y) { //game won
                    _stopWatch.Stop();
                    IsGameWon = true;
                } else if (cell.Value == CellValue.Empty) { //if there are no nearby mines, open all nearby cells automatically
                    foreach (var nearbyCellPoint in GetNearbyCellPoints(point))
                        OpenCell(nearbyCellPoint, ref changedCells);
                }
            }
        }

        /// <summary>
        /// Toggles flag on the given cell
        /// </summary>
        /// <param name="point">Point for the operation</param>
        private void ToggleFlag(PointI point)
        {
            var cell = Cells[point.X][point.Y];
            switch (cell.Status) {
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
        private void OpenNearbyCells(PointI point, ref List<PointI> changedCells)
        {
            var cell = Cells[point.X][point.Y];
            if (cell.Status == CellStatus.Covered) {
                OpenCell(point, ref changedCells); //if the cell is still covered, open it
            } else if (cell.Status == CellStatus.Opened) {
                int flagCounter = 0;
                var nearbyCellPoints = GetNearbyCellPoints(point);
                foreach (var nearbyCellPoint in nearbyCellPoints) //count the flags on nearby cells
                    if (Cells[nearbyCellPoint.X][nearbyCellPoint.Y].Status == CellStatus.Flagged)
                        flagCounter++;
                if (flagCounter == (int)cell.Value) { //if the flag count on nearby cells equal to the value of mines nearby open nearby cells
                    foreach (var nearbyCellPoint in nearbyCellPoints)
                        OpenCell(nearbyCellPoint, ref changedCells);
                }
            }
        }

        /// <summary>
        /// On game over, undo the last operation, so that the game keeps going
        /// </summary>
        /// <returns>the manipulated cell</returns>
        public List<PointI> Undo()
        {
            if(IsGameOver) {
                IsGameOver = false;
                _stopWatch.Start();
                Cells[_lastOpenedPoint.X][_lastOpenedPoint.Y].Status = CellStatus.Covered;
                NonMinesOpened--;
                return new List<PointI>() { _lastOpenedPoint };
            }
            return new List<PointI>();
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
        /// <param name="reversed">swaps height and width</param>
        /// <returns>returns a field with certain options</returns>
        public static Field GetStandardsField(Standards predefinedField, bool reversed = false) //https://www.bernhard-gaul.de/spiele/minesweeper/minesweeper-spielregel.html
        {
            switch (predefinedField) {
                case Standards.Beginner:
                    return new Field(9, 9, 10, reversed);
                case Standards.Intermediate:
                    return new Field(16, 16, 40, reversed);
                case Standards.Expert:
                    return new Field(30, 16, 99, reversed);
            }

            return null;
        }

        /// <summary>
        /// Check if Field-Coordinate exists in Current Field
        /// </summary>
        /// <param name="fieldCoordinate"></param>
        /// <returns></returns>
        public bool FieldExists(PointI fieldCoordinate)
        {
            if (fieldCoordinate.X < 0 || fieldCoordinate.Y < 0 || fieldCoordinate.X >= this.X || fieldCoordinate.Y >= this.Y)
                return false;
            return true;
        }
    }
}

public static class Extensions
{
    /// <summary>
    /// Gets the description of the enum if available otherwise its name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e">the enumValue</param>
    /// <returns>Description of the enum if available otherwise its name</returns>
    public static string ToDescription<T>(this T e) where T : IConvertible
    {
        var type = e.GetType();
        Array values = Enum.GetValues(type);

        foreach (int val in values) {
            if (val == e.ToInt32(CultureInfo.InvariantCulture)) {
                var memInfo = type.GetMember(type.GetEnumName(val));
                var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (descriptionAttributes.Length > 0) {
                    // we're only getting the first description we find
                    // others will be ignored
                    return ((DescriptionAttribute)descriptionAttributes[0]).Description;
                }
            }
        }
        return Enum.GetName(type, e);
    }
}