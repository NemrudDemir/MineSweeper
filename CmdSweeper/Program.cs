using SweeperModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Just a bonus ;)
/// </summary>
namespace CmdSweeper
{
    class Program
    {
        static void Main(string[] args)
        {
            NewGame();
        }

        static Field field;
        static PointI focusedPoint;

        static void NewGame()
        {
            Console.Clear();
            Console.WriteLine($"Breite: ({Field.MinX} - {Field.MaxX})");
            int x;
            while (!int.TryParse(Console.ReadLine(), out x) || x < Field.MinX || x > Field.MaxX) {
                Console.WriteLine("Angabe ungültig, bitte erneut versuchen");
            }

            Console.WriteLine($"Höhe: ({Field.MinY} - {Field.MaxY})");
            int y;
            while (!int.TryParse(Console.ReadLine(), out y) || y < Field.MinY || y > Field.MaxY) {
                Console.WriteLine("Angabe ungültig, bitte erneut versuchen");
            }

            Console.WriteLine($"Anzahl der Minen: ({Field.MinMines} - {Field.GetMaxMines(x,y)})");
            int mines;
            while (!int.TryParse(Console.ReadLine(), out mines) || mines < Field.MinMines || mines > Field.GetMaxMines(x, y)) {
                Console.WriteLine("Angabe ungültig, bitte erneut versuchen");
            }

            SetField(y, x, mines);
            NextStep();
        }

        static void SetField(int x, int y, int mines)
        {
            field = new Field(x, y, mines);
            focusedPoint = new PointI(0,0);
        }

        static string[] newGameOptions = new string[] { "Erneut spielen", "Benutzerdefiniert", "Beenden" };

        static int gameOverIndexFocused = 0;
        static void NextStep()
        {
            Console.Clear();
            var cells = field.Cells;
            foreach (var cellA in cells) {
                foreach (var cell in cellA) {
                    string text = $"{GetCharForCell(cell)} ";
                    if (cell == field.Cells[focusedPoint.X][focusedPoint.Y])
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(text);
                }
                Console.WriteLine();
            }
            if (field.IsGameOver ||field.IsGameWon) {
                if (field.IsGameOver) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("GAME OVER!");
                } else if(field.IsGameWon) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("GAME WON!");
                }
                for (int i = 0; i < newGameOptions.Length; i++) {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                    if (i == gameOverIndexFocused) {
                        var temp = Console.ForegroundColor;
                        Console.ForegroundColor = Console.BackgroundColor;
                        Console.BackgroundColor = temp;
                    }
                    Console.Write(newGameOptions[i]);
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
            switch (key.Key) {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    if(focusedPoint.X == 0) {
                        DoMove();
                        return;
                    }
                    focusedPoint.X--;
                    break;
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    if (field.IsGameOver || field.IsGameWon) {
                        if(gameOverIndexFocused == 0) {
                            DoMove();
                            return;
                        }
                        gameOverIndexFocused--;
                    } else {
                        if (focusedPoint.Y == 0) {
                            DoMove();
                            return;
                        }
                        focusedPoint.Y--;
                    }
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    if (focusedPoint.X == field.X-1) {
                        DoMove();
                        return;
                    }
                    focusedPoint.X++;
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    if (field.IsGameOver || field.IsGameWon) {
                        if (gameOverIndexFocused == 2) {
                            DoMove();
                            return;
                        }
                        gameOverIndexFocused++;
                    } else {
                        if (focusedPoint.Y == field.Y - 1) {
                            DoMove();
                            return;
                        }
                        focusedPoint.Y++;
                    }
                    break;
                case ConsoleKey.F:
                case ConsoleKey.Insert:
                    field.DoOperation(focusedPoint, Field.Mode.Flag);
                    break;
                case ConsoleKey.Spacebar:
                case ConsoleKey.Enter:
                    if (field.IsGameOver || field.IsGameWon) {
                        if(gameOverIndexFocused == 0) {
                            SetField(field.X, field.Y, field.MinesTotal);
                        } else if(gameOverIndexFocused == 1) {
                            NewGame();
                        } else {
                            return;
                        }
                    } else
                        field.DoOperation(focusedPoint, Field.Mode.Open);
                    break;
                default:
                    DoMove();
                    return;
            }
            NextStep();
        }

        static char GetCharForCell(Cell cell)
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        return '3';
                    case CellValue.Four:
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        return '4';
                    case CellValue.Five:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        return '5';
                    case CellValue.Six:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        return '6';
                    case CellValue.Seven:
                        Console.ForegroundColor = ConsoleColor.Magenta;
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
