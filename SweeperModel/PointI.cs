namespace SweeperModel
{
    /// <summary>
    /// Integer Point
    /// </summary>
    public class PointI
    {
        /// <summary>
        /// Gets or sets the X coordinate
        /// </summary>
        public int X {
            get; set;
        }

        /// <summary>
        /// Gets or sets the Y coordinate
        /// </summary>
        public int Y {
            get; set;
        }

        public PointI(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
