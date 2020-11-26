using System;
using SweeperModel;
using SweeperModel.Elements;

namespace CmdSweeper.Extensions
{
    public static class CellExtensions
    {
        public static char GetChar(this Cell cell)
        {
            if(cell.Status == CellStatus.Covered)
                return '█';

            if(cell.Status == CellStatus.Flagged)
                return '┌';

            switch(cell.Value)
            {
                case CellValue.Mine:
                    return 'x';
                case CellValue.Empty:
                    return '█';
                case CellValue.One:
                    return '1';
                case CellValue.Two:
                    return '2';
                case CellValue.Three:
                    return '3';
                case CellValue.Four:
                    return '4';
                case CellValue.Five:
                    return '5';
                case CellValue.Six:
                    return '6';
                case CellValue.Seven:
                    return '7';
                case CellValue.Eight:
                    return '8';
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ConsoleColor GetColor(this Cell cell)
        {
            if(cell.Status == CellStatus.Covered)
                return ConsoleColor.DarkGray;

            if(cell.Status == CellStatus.Flagged)
                return ConsoleColor.DarkRed;

            switch(cell.Value)
            {
                case CellValue.Mine:
                    return Console.ForegroundColor; //TODO change this
                case CellValue.Empty:
                    return ConsoleColor.Black;
                case CellValue.One:
                    return ConsoleColor.Blue;
                case CellValue.Two:
                    return ConsoleColor.Green;
                case CellValue.Three:
                    return ConsoleColor.Yellow;
                case CellValue.Four:
                    return ConsoleColor.DarkBlue;
                case CellValue.Five:
                    return ConsoleColor.Magenta;
                case CellValue.Six:
                    return ConsoleColor.Cyan;
                case CellValue.Seven:
                    return ConsoleColor.Red;
                case CellValue.Eight:
                    return ConsoleColor.DarkGray;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
