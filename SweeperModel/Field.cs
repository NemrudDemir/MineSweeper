using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static int MinX {
            get { return 9; }
        }

        /// <summary>
        /// Minimum Height
        /// </summary>
        public static int MinY {
            get { return 9; }
        }

        /// <summary>
        /// Maximum Width
        /// </summary>
        public static int MaxX {
            get { return 30; }
        }

        /// <summary>
        /// Maximum Height
        /// </summary>
        public static int MaxY {
            get { return 24; }
        }

        /// <summary>
        /// Minimum Mines
        /// </summary>
        public static int MinMines {
            get { return 10; }
        }

        /// <summary>
        /// Gets the minimum mines 
        /// </summary>
        /// <param name="x">width of field</param>
        /// <param name="y">height of field</param>
        /// <returns></returns>
        public static int GetMinMines(int x, int y)
        {
            return MinMines;
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

        private Stopwatch stopWatch = new Stopwatch();
        /// <summary>
        /// Get Elapsed Milliseconds
        /// </summary>
        public long GetTime {
            get { return stopWatch.ElapsedMilliseconds; }
        }

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
        private int minesLeft;
        public int MinesLeft {
            get { return minesLeft; }
        }

        /// <summary>
        /// Indicator for how many non-mines cells opened on the field
        /// </summary>
        private int nonMinesOpened;
        public int NonMinesOpened {
            get { return nonMinesOpened; }
        }

        public Cell[][] Cells {
            get; set;
        }
        private bool initializedField = false;

        private PointI lastOpenedPoint; //is required for redo operation
        private bool isGameOver = false;
        public bool IsGameOver {
            get { return isGameOver; }
        }

        private bool isGameWon = false;
        public bool IsGameWon {
            get { return isGameWon; }
        }

        /// <summary>
        /// Initializes a field
        /// </summary>
        /// <param name="x">width</param>
        /// <param name="y">heigh</param>
        /// <param name="mines">number of mines</param>
        public Field(int x, int y, int mines)
        {
            X = x;
            Y = y;
            MinesTotal = minesLeft = mines;

            if (mines > GetMaxMines(x, y))
                throw new Exception("There are too many mines for the fieldsize!");

            //Initialize cells
            Cells = new Cell[x][];
            for (int i = 0; i < x; i++) {
                Cells[i] = new Cell[Y];
                for (int j = 0; j < y; j++)
                    Cells[i][j] = new Cell();
            }
        }

        /// <summary>
        /// Setting the mines on the field
        /// </summary>
        /// <param name="midPoint">point hit</param>
        public void SetMines(PointI midPoint)
        {
            initializedField = true;
            int minesToSet = MinesTotal; //Anzahl Minen die gesetzt werden dürfen
            int[] protectedCells = GetNearbyCellPoints(midPoint, true).Select(x => GenerateNumberFromPoint(x)).ToArray(); //Die angeklickte Zelle sowie die umstehenden Zellen sollen keine Minen sein!
            int cellsAvailable = X * Y - protectedCells.Length; //Anzahl der Potenziellen Zellen welche eine Mine beinhalten können
            List<int> minesAt = new List<int>(); //Liste der Indizes, in denen sich Minen beinhalten
            minesAt.AddRange(protectedCells); //Sind nur Pseudo-Einträge, da diese Zellen keine Mine beinhalten dürfen - welche später wieder rausgefiltert werden
            while(minesToSet > 0) {
                //Indizes für jede Mine generieren
                Random random = new Random(Guid.NewGuid().GetHashCode());
                int mineIndex = random.Next(cellsAvailable);
                int index = 0;
                foreach(var mI in minesAt) {
                    if (mineIndex >= mI)
                        mineIndex++;
                    else
                        break;
                    index++;
                }
                minesAt.Insert(index, mineIndex);
                minesToSet--;
                cellsAvailable--;
            }

            foreach(int mine in minesAt)
                if(!protectedCells.Contains(mine)) //Pseudo Einträge sind keine Minen!
                    SetMine(mine);

            stopWatch.Start();
        }

        /// <summary>
        /// Set cell as mine
        /// </summary>
        /// <param name="index">Index of the mine</param>
        private void SetMine(int index)
        {
            var point = GeneratePointFromNumber(index);
            Cells[point.X][point.Y].SetAsMine();
            UpdateNearbyCells(point.X, point.Y);
        }

        /// <summary>
        /// Gets all the points nearby the given point
        /// </summary>
        /// <param name="midPoint">middle Point</param>
        /// <param name="withOwnCell">Adds the given point to the collection</param>
        /// <returns></returns>
        private PointI[] GetNearbyCellPoints(PointI midPoint, bool withOwnCell=false)
        {
            int x = midPoint.X;
            int y = midPoint.Y;
            List<PointI> nearbyCells = new List<PointI>();
            nearbyCells.Add(GetCellPoint(x - 1, y - 1)); //top left
            nearbyCells.Add(GetCellPoint(x, y - 1)); //top
            nearbyCells.Add(GetCellPoint(x + 1, y - 1)); //top right
            nearbyCells.Add(GetCellPoint(x - 1, y)); //left
            if (withOwnCell)
                nearbyCells.Add(midPoint); //own
            nearbyCells.Add(GetCellPoint(x + 1, y)); // right
            nearbyCells.Add(GetCellPoint(x - 1, y + 1)); //bottom left
            nearbyCells.Add(GetCellPoint(x, y + 1)); //bottom
            nearbyCells.Add(GetCellPoint(x + 1, y + 1)); //bottom right

            return nearbyCells.Where(point => point != null).ToArray();
        }

        /// <summary>
        /// Gets all the cells nearby the given point
        /// </summary>
        /// <param name="point">middle Point</param>
        /// <param name="withOwnCell">Adds the given point-cell to the collection</param>
        /// <returns></returns>
        private Cell[] GetNearbyCells(PointI point, bool withOwnCell=false)
        {
            return GetNearbyCellPoints(point, withOwnCell).Select(cPoint => Cells[cPoint.X][cPoint.Y]).ToArray();
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
        /// <param name="x">x coordinate of cell</param>
        /// <param name="y">y coordinate of cell</param>
        private void UpdateNearbyCells(int x, int y)
        {
            foreach (var nearbyCell in GetNearbyCells(new PointI(x, y)))
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
            if (isGameOver || isGameWon)
                return new List<PointI>();
            if (!initializedField)
                SetMines(point);

            List<PointI> changedCells = new List<PointI>();
            var OperationCell = Cells[point.X][point.Y];
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
            if(cell.Status == CellStatus.Covered && !isGameOver) { //only covered cells can be opened
                lastOpenedPoint = point;
                cell.Status = CellStatus.Opened;
                changedCells.Add(point);
                nonMinesOpened++;
                if (cell.Value == CellValue.Mine) { //game over
                    stopWatch.Stop();
                    isGameOver = true;
                } else if (NonMinesOpened + MinesTotal == X*Y) { //game won
                    stopWatch.Stop();
                    isGameWon = true;
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
            Cell cell = Cells[point.X][point.Y];
            switch (cell.Status) {
                case CellStatus.Covered:
                    cell.Status = CellStatus.Flagged;
                    minesLeft--;
                    break;
                case CellStatus.Flagged:
                    cell.Status = CellStatus.Covered;
                    minesLeft++;
                    break;
                default:
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
            Cell cell = Cells[point.X][point.Y];
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
        /// On game over, redo the last operation, so that the game keeps going
        /// </summary>
        /// <returns>the manipulated cell</returns>
        public List<PointI> Redo()
        {
            if(IsGameOver) {
                isGameOver = false;
                stopWatch.Start();
                Cells[lastOpenedPoint.X][lastOpenedPoint.Y].Status = CellStatus.Covered;
                return new List<PointI>() { lastOpenedPoint };
            }
            return new List<PointI>();
        }

        /// <summary>
        /// Standard fields
        /// </summary>
        public enum Standards
        {
            [Description("Anfänger")]
            Beginner,
            [Description("Fortgeschritten")]
            Intermediate,
            [Description("Experte")]
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
        /// <returns></returns>
        public static Field GetStandardsField(Standards predefinedField) //https://www.bernhard-gaul.de/spiele/minesweeper/minesweeper-spielregel.html
        {
            switch (predefinedField) {
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
        Type type = e.GetType();
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