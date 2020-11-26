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
            if(Value != CellValue.Mine)
                Value++;
        }
    }
}
