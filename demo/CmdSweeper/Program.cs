using System;
using SweeperModel;
using SweeperModel.Elements;

namespace CmdSweeper
{
    class Program
    {
        private static Field _field;
        private static (int X, int Y) _focusedPoint;

        static void Main()
        {
            NewGame();
        }

        private static void NewGame()
        {
            Console.Clear();
            Console.WriteLine($"Breite: ({Field.MIN_XY} - {Field.MAX_XY})");
            int x;
            while (!int.TryParse(Console.ReadLine(), out x) || x < Field.MIN_XY || x > Field.MAX_XY) {
                Console.WriteLine("Angabe ungültig, bitte erneut versuchen");
            }

            Console.WriteLine($"Höhe: ({Field.MIN_XY} - {Field.MAX_XY})");
            int y;
            while (!int.TryParse(Console.ReadLine(), out y) || y < Field.MIN_XY || y > Field.MAX_XY) {
                Console.WriteLine("Angabe ungültig, bitte erneut versuchen");
            }

            Console.WriteLine($"Anzahl der Minen: ({Field.GetMinMines()} - {Field.GetMaxMines(x,y)})");
            int mines;
            while (!int.TryParse(Console.ReadLine(), out mines) || mines < Field.GetMinMines() || mines > Field.GetMaxMines(x, y))
                Console.WriteLine("Angabe ungültig, bitte erneut versuchen");

            SetField(y, x, mines);
            NextStep();
        }

        private static void SetField(int x, int y, int mines)
        {
            _field = new Field(x, y, mines);
            _focusedPoint = (0,0);
        }

        private static int _gameOverFocusedIndex;
        private static readonly Option[] GameOverOptions = new [] {
            new Option("Erneut spielen", Restart),
            new Option("Benutzerdefiniert", NewGame),
            new Option("Beenden", Exit)
        };

        /// <summary>
        /// Exits the application
        /// </summary>
        private static void Exit()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Starts a new game with the same game options
        /// </summary>
        private static void Restart()
        {
            
            SetField(_field.X, _field.Y, _field.MinesTotal);
        }

        static void NextStep()
        {
            Console.Clear();
            var cells = _field.Cells;
            foreach (var cellA in cells) {
                foreach (var cell in cellA) {
                    string text = $"{GetCharForCell(cell)} ";
                    if (cell == _field.Cells[_focusedPoint.X][_focusedPoint.Y])
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(text);
                }
                Console.WriteLine();
            }
            if (_field.IsGameOver ||_field.IsGameWon) {
                if (_field.IsGameOver) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("GAME OVER!");
                } else if(_field.IsGameWon) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("GAME WON!");
                }
                for (int i = 0; i < GameOverOptions.Length; i++) {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                    if (i == _gameOverFocusedIndex) {
                        var temp = Console.ForegroundColor;
                        Console.ForegroundColor = Console.BackgroundColor;
                        Console.BackgroundColor = temp;
                    }
                    Console.Write(GameOverOptions[i].Caption);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("\t");
                }
            } else {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Navigation: W, A, S, D oder Pfeiltasten  Aktionen: Space/Enter -> Öffnen f, einfg -> Flag");
            }
            DoMove();
        }

        static void DoMove()
        {
            var key = Console.ReadKey();
            switch (key.Key) { //TODO find better way for implementation
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    if(_focusedPoint.X == 0) {
                        DoMove();
                        return;
                    }
                    _focusedPoint.X--;
                    break;
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    if (_field.IsGameOver || _field.IsGameWon) {
                        if(_gameOverFocusedIndex == 0) {
                            DoMove();
                            return;
                        }
                        _gameOverFocusedIndex--;
                    } else {
                        if (_focusedPoint.Y == 0) {
                            DoMove();
                            return;
                        }
                        _focusedPoint.Y--;
                    }
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    if (_focusedPoint.X == _field.X-1) {
                        DoMove();
                        return;
                    }
                    _focusedPoint.X++;
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    if (_field.IsGameOver || _field.IsGameWon) {
                        if (_gameOverFocusedIndex == 2) {
                            DoMove();
                            return;
                        }
                        _gameOverFocusedIndex++;
                    } else {
                        if (_focusedPoint.Y == _field.Y - 1) {
                            DoMove();
                            return;
                        }
                        _focusedPoint.Y++;
                    }
                    break;
                case ConsoleKey.F:
                case ConsoleKey.Insert:
                    _field.DoOperation(GetPointFromTuple(_focusedPoint), Field.Mode.Flag);
                    break;
                case ConsoleKey.Spacebar:
                case ConsoleKey.Enter:
                    if (_field.IsGameOver || _field.IsGameWon) {
                        GameOverOptions[_gameOverFocusedIndex].Action.Invoke();
                    } else {
                        var cell = _field.Cells[_focusedPoint.X][_focusedPoint.Y];
                        var point = GetPointFromTuple(_focusedPoint);
                        if (cell.Status == CellStatus.Covered)
                            _field.DoOperation(point, Field.Mode.Open);
                        else if (cell.Status == CellStatus.Opened)
                            _field.DoOperation(point, Field.Mode.OpenNearby);
                    }
                    break;
                default:
                    DoMove();
                    return;
            }
            NextStep();
        }

        static PointI GetPointFromTuple((int x, int y) tuple) => new PointI(tuple.x, tuple.y);

        static char GetCharForCell(Cell cell) //TODO dont set the foregroundcolor here...
        {
            if (cell.Status == CellStatus.Covered) {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                return '█';
            } else if (cell.Status == CellStatus.Flagged) {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                return '┌';
            } else {
                switch (cell.Value) {
                    case CellValue.Mine:
                        break;
                    case CellValue.Empty:
                        Console.ForegroundColor = ConsoleColor.Black;
                        return '█';
                    case CellValue.One:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        return '1';
                    case CellValue.Two:
                        Console.ForegroundColor = ConsoleColor.Green;
                        return '2';
                    case CellValue.Three:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        return '3';
                    case CellValue.Four:
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        return '4';
                    case CellValue.Five:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        return '5';
                    case CellValue.Six:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        return '6';
                    case CellValue.Seven:
                        Console.ForegroundColor = ConsoleColor.Red;
                        return '7';
                    case CellValue.Eight:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        return '8';
                }
            }
            return 'x';
        }
    }
}
