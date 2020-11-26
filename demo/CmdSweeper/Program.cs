using System;
using System.Collections.Generic;
using CmdSweeper.Extensions;
using SweeperModel;
using SweeperModel.Elements;
using static CmdSweeper.Constants;

namespace CmdSweeper
{
    internal class Program
    {
        private static Field _field;
        private static (int X, int Y) _focusedPoint;

        private static void Main()
        {
            NewGame();
        }

        private static void NewGame()
        {
            Console.Clear();
            Console.ResetColor();
            Console.WriteLine($"Width: ({FieldSize.MIN_XY} - {FieldSize.MAX_XY})");
            int x;
            while(!int.TryParse(Console.ReadLine(), out x) || x < FieldSize.MIN_XY || x > FieldSize.MAX_XY)
                Console.WriteLine(INVALID_INPUT_TRY_AGAIN);

            Console.WriteLine($"Height: ({FieldSize.MIN_XY} - {FieldSize.MAX_XY})");
            int y;
            while(!int.TryParse(Console.ReadLine(), out y) || y < FieldSize.MIN_XY || y > FieldSize.MAX_XY)
                Console.WriteLine(INVALID_INPUT_TRY_AGAIN);

            Console.WriteLine($"Number of mines: ({FieldSize.GetMinMines()} - {FieldSize.GetMaxMines(x, y)})");
            int mines;
            while(!int.TryParse(Console.ReadLine(), out mines) || mines < FieldSize.GetMinMines() || mines > FieldSize.GetMaxMines(x, y))
                Console.WriteLine(INVALID_INPUT_TRY_AGAIN);

            var fieldSize = new FieldSize(x, y, mines);
            SetField(fieldSize);
        }

        private static void SetField(FieldSize size)
        {
            _field = new Field(size);
            Console.SetWindowSize(Math.Max(Console.WindowWidth, size.X) * 2, Math.Max(Console.WindowHeight, size.Y + 5));
            _focusedPoint = (0, 0);
            Draw();
            ReadMove();
        }

        private static int _gameOverFocusedIndex;
        private static readonly Option[] GameOverOptions = new[] {
            new Option("Play again", Restart),
            new Option("Customized", NewGame),
            new Option("Exit", Exit)
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
            SetField(_field.Size);
        }

        private static IEnumerable<PointI> GetAllPoints()
        {
            for(var x = 0; x < _field.Size.X; x++)
                for(var y = 0; y < _field.Size.Y; y++)
                    yield return new PointI(x, y);
        }

        private static void Draw()
        {
            Console.Clear();
            Draw(GetAllPoints());
        }

        private static void Draw(IEnumerable<PointI> points)
        {
            foreach(var point in points)
            {
                Console.SetCursorPosition(point.X * 2, point.Y);
                var cell = _field.Cells[point.X, point.Y];
                var text = $"{cell.GetChar()} ";
                Console.ForegroundColor = cell.GetColor();
                if(cell == _field.Cells[_focusedPoint.X, _focusedPoint.Y])
                    Console.ForegroundColor = ConsoleColor.White;
                Console.Write(text);
            }
            Console.SetCursorPosition(0, _field.Size.Y);
            if(_field.GameStatus != GameStatus.Running)
            {
                Console.WriteLine("");
                if(_field.GameStatus == GameStatus.Lost)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("GAME OVER!");
                }
                else if(_field.GameStatus == GameStatus.Won)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("GAME WON!");
                }
                for(int i = 0; i < GameOverOptions.Length; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                    if(i == _gameOverFocusedIndex)
                    {
                        var temp = Console.ForegroundColor;
                        Console.ForegroundColor = Console.BackgroundColor;
                        Console.BackgroundColor = temp;
                    }
                    Console.Write(GameOverOptions[i].Caption);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("\t");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Navigation: 'W', 'A', 'S', 'D' or arrow keys | Open cell: space or return | Flag cell: 'F' or Insert");
            }
        }

        private static void ReadMove()
        {
            while(true)
            {
                var key = Console.ReadKey();
                var changedPoints = new List<PointI> { _focusedPoint.Point() };
                //TODO find better way for implementation
                if(_field.GameStatus != GameStatus.Running)
                {
                    switch(key.Key)
                    {
                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            if(_gameOverFocusedIndex == 0)
                                continue;
                            _gameOverFocusedIndex--;
                            break;
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            if(_gameOverFocusedIndex == GameOverOptions.Length - 1)
                                continue;
                            _gameOverFocusedIndex++;
                            break;
                        case ConsoleKey.Spacebar:
                        case ConsoleKey.Enter:
                            GameOverOptions[_gameOverFocusedIndex].Action.Invoke();
                            break;
                    }
                }
                else
                {
                    switch(key.Key)
                    {
                        case ConsoleKey.W:
                        case ConsoleKey.UpArrow:
                            if(_focusedPoint.Y == 0)
                                continue;
                            _focusedPoint.Y--;
                            break;
                        case ConsoleKey.A:
                        case ConsoleKey.LeftArrow:
                            if(_focusedPoint.X == 0)
                                continue;
                            _focusedPoint.X--;
                            break;
                        case ConsoleKey.S:
                        case ConsoleKey.DownArrow:
                            if(_focusedPoint.Y == _field.Size.Y - 1)
                                continue;
                            _focusedPoint.Y++;
                            break;
                        case ConsoleKey.D:
                        case ConsoleKey.RightArrow:
                            if(_focusedPoint.X == _field.Size.X - 1)
                                continue;
                            _focusedPoint.X++;
                            break;
                        case ConsoleKey.F:
                        case ConsoleKey.Insert:
                            changedPoints = _field.DoOperation(_focusedPoint.Point(), Field.Mode.Flag);
                            break;
                        case ConsoleKey.Spacebar:
                        case ConsoleKey.Enter:
                            var cell = _field.Cells[_focusedPoint.X, _focusedPoint.Y];
                            var point = _focusedPoint.Point();
                            if(cell.Status == CellStatus.Covered)
                                changedPoints = _field.DoOperation(point, Field.Mode.Open);
                            else if(cell.Status == CellStatus.Opened)
                                changedPoints = _field.DoOperation(point, Field.Mode.OpenNearby);
                            break;
                    }
                }
                changedPoints.Add(_focusedPoint.Point());
                Draw(changedPoints);
            }
        }
    }
}
