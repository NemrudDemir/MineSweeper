using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweeperModel
{
    public class Cell
    {
        CellValue value;
        public CellValue Value {
            get { return value; }
        }

        public CellStatus Status {
            get; set;
        }

        public Cell()
        {
            this.value = CellValue.Empty;
            this.Status = CellStatus.Covered;
        }

        /// <summary>
        /// For pseudo cells only!
        /// </summary>
        public Cell(CellValue value)
        {
            this.value = value;
        }

        /// <summary>
        /// Sets this cell as mine
        /// </summary>
        public void SetAsMine()
        {
            value = CellValue.Mine;
        }

        /// <summary>
        /// Increments the value of this cell
        /// </summary>
        public void AddNearbyMine()
        {
            if (value != CellValue.Mine)
                value++;
        }
    }

    /// <summary>
    /// The status of a cell
    /// </summary>
    public enum CellStatus
    {
        Covered,
        Flagged,
        Opened
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
        Eight = 8
    }
}
