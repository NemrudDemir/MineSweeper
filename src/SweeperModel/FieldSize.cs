using System.Collections.Generic;
using SweeperModel.Exceptions;

namespace SweeperModel
{
    public class FieldSize
    {
        public const int MIN_XY = 9;
        public const int MAX_XY = 30;
        private const int MIN_MINES = 10;

        public static readonly FieldSize Beginner = new FieldSize(9, 9, 10, nameof(Beginner));
        public static readonly FieldSize Intermediate = new FieldSize(16, 16, 40, nameof(Intermediate));
        public static readonly FieldSize Expert = new FieldSize(30, 16, 99, nameof(Expert));
        public static IEnumerable<FieldSize> Standards {
            get {
                yield return Beginner;
                yield return Intermediate;
                yield return Expert;
            }
        }

        private string _name;
        public string Name {
            get {
                if(string.IsNullOrEmpty(_name))
                    _name = $"Custom (x = {X}, y = {Y}, mines = {MinesTotal}";
                return _name;
            }
            set => _name = value;
        }
        public int X { get; }
        public int Y { get; }
        public int MinesTotal { get; }

        public FieldSize(int x, int y, int minesTotal) : this(x, y, minesTotal, string.Empty)
        {
        }
        public FieldSize(int x, int y, int minesTotal, string name)
        {
            Name = name;
            if(minesTotal > GetMaxMines(x, y) || minesTotal < GetMinMines() || x > MAX_XY || y > MAX_XY ||
                x < MIN_XY || y < MIN_XY)
                throw new InvalidFieldSizeException();

            X = x;
            Y = y;
            MinesTotal = minesTotal;
        }

        public override string ToString()
        {
            return Name;
        }

        public static int GetMinMines() => MIN_MINES;
        public static int GetMaxMines(int x, int y)
        {
            return x * y - (x + y - 1);
        }

    }
}
