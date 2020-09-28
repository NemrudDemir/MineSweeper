using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SweeperModel.Elements;

namespace SweeperModel.Test
{
    [TestClass]
    public class FieldTest
    {
        private const int X = 20;
        private const int Y = 10;
        private const int MINES = 35;

        [TestMethod]
        public void CreateField_ValidOptions_ShouldntThrow()
        {
            var field = new Field(X, Y, MINES);
            field.InitializeCellValues(new PointI(0, 0));

            Assert.AreEqual(X, field.X);
            Assert.AreEqual(Y, field.Y);
            var minesCount = field.Cells.Sum(row => row.Count(cell => cell.Value == CellValue.Mine));
            Assert.AreEqual(MINES, minesCount);
        }

        [TestMethod]
        public void CreateFieldReversed_ValidOptions_ShouldntThrow()
        {
            var field = new Field(X, Y, MINES, true);
            field.InitializeCellValues(new PointI(0, 0));

            Assert.AreEqual(Y, field.X);
            Assert.AreEqual(X, field.Y);
            var minesCount = field.Cells.Sum(row => row.Count(cell => cell.Value == CellValue.Mine));
            Assert.AreEqual(MINES, minesCount);
        }

        [TestMethod]
        public void CreateField_ValidOptionsMaxBound_ShouldntThrow()
        {
            var max = Field.MAX_XY;
            var field = new Field(max, max, Field.GetMaxMines(max, max));
            field.InitializeCellValues(new PointI(0, 0));
        }

        [TestMethod]
        public void CreateField_ValidOptionsMinBound_ShouldntThrow()
        {
            var min = Field.MIN_XY;
            var field = new Field(min, min, Field.GetMinMines());
            field.InitializeCellValues(new PointI(0, 0));
        }

        [TestMethod]
        public void CreateField_InvalidOptions_ShouldThrow()
        {
            Assert.ThrowsException<Exception>(() => new Field(X, Y, -1));
            Assert.ThrowsException<Exception>(() => new Field(0, Y, MINES));
            Assert.ThrowsException<Exception>(() => new Field(X, 0, MINES));
        }

        [TestMethod]
        public void DoOperation_WhileNotInitialized_ShouldInitialize()
        {
            var field = new Field(X, Y, MINES);
            var cells = new List<Cell>();
            foreach(var cellX in field.Cells)
                foreach(var cell in cellX)
                    cells.Add(cell);
            Assert.AreEqual(0, cells.Count(cell => cell.Value == CellValue.Mine));
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            Assert.AreEqual(MINES, cells.Count(cell => cell.Value == CellValue.Mine));
        }

        [TestMethod]
        public void OpenCell_FirstOpen_ShouldOpenMoreThanOneCell()
        {
            var field = new Field(X, Y, MINES);
            var changedCells = field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            Assert.IsTrue(changedCells.Count > 1);
        }

        [TestMethod]
        public void OpenCell_MineOpen_ShouldBeGameOver()
        {
            var field = new Field(X, Y, MINES);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            for (var row = 0; row < field.Cells.Length; row++)
            {
                for (var column = 0; column < field.Cells[row].Length; column++)
                {
                    var cell = field.Cells[row][column];
                    if (cell.Value == CellValue.Mine)
                    {
                        field.DoOperation(new PointI(row, column), Field.Mode.Open);
                        Assert.AreEqual(GameStatus.Lost, field.GameStatus);
                        return;
                    }
                }
            }

            throw new Exception();
        }

        [TestMethod]
        public void OpenCell_OpenAllNonMines_ShouldBeGameWon()
        {
            var field = new Field(X, Y, MINES);
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            for (var row = 0; row < field.Cells.Length; row++)
            {
                for (var column = 0; column < field.Cells[row].Length; column++)
                {
                    var cell = field.Cells[row][column];
                    if (cell.Value != CellValue.Mine)
                    {
                        field.DoOperation(new PointI(row, column), Field.Mode.Open);
                    }
                }
            }

            Assert.AreEqual(GameStatus.Won, field.GameStatus);
        }

        [TestMethod]
        public void FlagCell_ShouldFlagCellAndDecreaseMineLeftCounter()
        {
            var field = new Field(X, Y, MINES);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            Assert.AreEqual(CellStatus.Flagged, field.Cells[0][0].Status);
            Assert.AreEqual(MINES-1, field.MinesLeft);
        }

        [TestMethod]
        public void FlagCellTwice_ShouldRemoveFlagCell()
        {
            var field = new Field(X, Y, MINES);
            Assert.AreEqual(MINES, field.MinesLeft);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            Assert.AreEqual(CellStatus.Covered, field.Cells[0][0].Status);
            Assert.AreEqual(MINES, field.MinesLeft);
        }

        [TestMethod]
        public void OpenNearby_OnCoveredCell_ShouldOpenCoveredCell()
        {
            var field = new Field(X, Y, MINES);
            field.DoOperation(new PointI(0, 0), Field.Mode.OpenNearby);
            Assert.AreEqual(CellStatus.Opened, field.Cells[0][0].Status);
        }

        [TestMethod]
        public void OpenNearby_OnNearbyFlagCountMatchesValue_ShouldOpenNearbyCovered()
        {
            var field = new Field(X, Y, 10);
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            for (var row = 0; row < field.Cells.Length; row++)
            {
                for (var column = 0; column < field.Cells[row].Length; column++)
                {
                    var cell = field.Cells[row][column];
                    if (cell.Value > CellValue.Empty)
                    {
                        var nearbyCellPoints = field.GetNearbyCellPoints(new PointI(row, column)).ToList();
                        var isCoveredPoint = default(PointI);
                        foreach (var nearbyPoint in nearbyCellPoints)
                        {
                            var nearbyCell = field.Cells[nearbyPoint.X][nearbyPoint.Y];
                            if (nearbyCell.Value == CellValue.Mine && nearbyCell.Status != CellStatus.Flagged)
                                field.DoOperation(nearbyPoint, Field.Mode.Flag);
                            else if (nearbyCell.Status == CellStatus.Covered)
                                isCoveredPoint = nearbyPoint;
                        }
                        if(isCoveredPoint == null) //all open already
                            continue;

                        Assert.IsTrue(field.Cells[isCoveredPoint.X][isCoveredPoint.Y].Status == CellStatus.Covered);
                        field.DoOperation(new PointI(row, column), Field.Mode.OpenNearby);
                        Assert.IsTrue(field.Cells[isCoveredPoint.X][isCoveredPoint.Y].Status == CellStatus.Opened);
                        return;
                    }
                }
            }

            throw new Exception("Make sure to hit 'return' in the loop");
        }

        [TestMethod]
        public void Undo_OnStartedGame_ShouldntChangeCells()
        {
            var field = new Field(X, Y, MINES);
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            var changed = field.Undo();
            Assert.IsNull(changed);
        }

        [TestMethod]
        public void Undo_OnGameOver_ShouldChangeCell()
        {
            var field = new Field(X, Y, MINES);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            for (var row = 0; row < field.Cells.Length; row++)
            {
                for (var column = 0; column < field.Cells[row].Length; column++)
                {
                    var cell = field.Cells[row][column];
                    if (cell.Value == CellValue.Mine)
                    {
                        field.DoOperation(new PointI(row, column), Field.Mode.Open);
                        Assert.AreEqual(GameStatus.Lost, field.GameStatus);
                        var changedCell = field.Undo();
                        Assert.IsTrue(changedCell.X == row && changedCell.Y == column);
                        return;
                    }
                }
            }

            throw new Exception();
        }

        [TestMethod]
        public void GetElapsed_BeforeStart_ShouldBeBiggerThanZero()
        {
            var field = new Field(X, Y, MINES);
            Thread.Sleep(100);
            Assert.AreEqual(0, field.GetElapsedMilliseconds);
        }

        [TestMethod]
        public void GetElapsed_AfterStart_ShouldBeBiggerThanZero()
        {
            var field = new Field(X, Y, MINES);
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            Thread.Sleep(100);
            Assert.IsTrue(field.GetElapsedMilliseconds > 0);
        }

        [TestMethod]
        public void GetStandardFields_ForEveryEnum_ShouldReturnFields()
        {
            foreach(Field.Standards predefinedField in Enum.GetValues(typeof(Field.Standards))) {
                var field = Field.GetStandardsField(predefinedField);
                Assert.IsNotNull(field);
            }
        }

        [TestMethod]
        public void GetStandardFields_ForInvalidStandard_ShouldReturnNull()
        {
            var field = Field.GetStandardsField((Field.Standards)(-1));
            Assert.IsNull(field);
        }
    }
}
