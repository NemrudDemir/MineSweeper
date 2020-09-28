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
            get;
        }

        /// <summary>
        /// Gets or sets the Y coordinate
        /// </summary>
        public int Y {
            get;
        }

        public PointI(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
