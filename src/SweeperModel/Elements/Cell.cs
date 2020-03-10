namespace SweeperModel.Elements
{
    public class Cell
    {
        /// <summary>
        /// Gets the Value of the Cell
        /// </summary>
        public CellValue Value {
            get; private set;
        }

        /// <summary>
        /// Gets the Status of the Cell
        /// </summary>
        public CellStatus Status {
            get; internal set;
        }

        public Cell()
        {
            Value = CellValue.Empty;
            Status = CellStatus.Covered;
        }

        /// <summary>
        /// Sets this cell as mine
        /// </summary>
        internal void SetAsMine()
        {
            Value = CellValue.Mine;
        }

        /// <summary>
        /// Increments the value of this cell
        /// </summary>
        internal void AddNearbyMine()
        {
            if (Value != CellValue.Mine)
                Value++;
        }
    }

    /// <summary>
    /// The status of a cell
    /// </summary>
    public enum CellStatus
    {
        Covered,
        Flagged,
        Opened,
    }

    /// <summary>
    /// The value of a cell
    /// </summary>
    public enum CellValue
    {
        Mine = -1,
        Empty = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
    }
}
