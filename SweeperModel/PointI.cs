using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SweeperModel
{
    /// <summary>
    /// Integer Point
    /// </summary>
    public class PointI
    {
        public int X {
            set; get;
        }

        public int Y {
            set; get;
        }

        public PointI(int x, int y)
        {
            X = x;
            Y = y;
        }

        public PointI(double x, double y) : this((int)x, (int)y) { }
    }
}
