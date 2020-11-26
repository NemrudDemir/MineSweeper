using SweeperModel;

namespace CmdSweeper.Extensions
{
    public static class TupleExtensions
    {
        public static PointI Point(this (int x, int y) tuple) => new PointI(tuple.x, tuple.y);
    }
}
