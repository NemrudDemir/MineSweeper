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
        private readonly FieldSize _size = new FieldSize(20, 10, 35);

        [TestMethod]
        public void CellMinesShouldEqualFieldSizeMinesAfterInitializeCellValues()
        {
            var field = new Field(_size);
            field.InitializeCellValues(new PointI(0, 0));
            var minesCount = field.Cells.OfType<Cell>().Count(cell => cell.Value == CellValue.Mine);
            Assert.AreEqual(_size.MinesTotal, minesCount);
        }

        [TestMethod]
        public void InitializeCellValuesOnFieldWithMaxBoundOptionsShouldNotThrow()
        {
            var max = FieldSize.MAX_XY;
            var size = new FieldSize(max, max, FieldSize.GetMaxMines(max, max));
            var field = new Field(size);
            field.InitializeCellValues(new PointI(0, 0));
        }

        [TestMethod]
        public void InitializeCellValuesOnFieldWithMinBoundOptionsShouldNotThrow()
        {
            var min = FieldSize.MIN_XY;
            var size = new FieldSize(min, min, FieldSize.GetMinMines());
            var field = new Field(size);
            field.InitializeCellValues(new PointI(0, 0));
        }

        [TestMethod]
        public void DoOperationWhileNotInitializedShouldInitialize()
        {
            var field = new Field(_size);
            var cells = field.Cells.OfType<Cell>().ToList();
            Assert.AreEqual(0, cells.Count(cell => cell.Value == CellValue.Mine));
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            Assert.AreEqual(_size.MinesTotal, cells.Count(cell => cell.Value == CellValue.Mine));
        }

        [TestMethod]
        public void OpenCellFirstOpenShouldOpenMoreThanOneCell()
        {
            var field = new Field(_size);
            var changedCells = field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            Assert.IsTrue(changedCells.Count > 1);
        }

        [TestMethod]
        public void OpenCellMineOpenShouldBeGameOver()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);

            for(var row = 0; row < field.Cells.GetLength(0); row++)
            {
                for(var column = 0; column < field.Cells.GetLength(1); column++)
                {
                    var cell = field.Cells[row, column];
                    if(cell.Value == CellValue.Mine)
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
        public void OpenCellOpenAllNonMinesShouldBeGameWon()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            for(var row = 0; row < field.Cells.GetLength(0); row++)
            {
                for(var column = 0; column < field.Cells.GetLength(1); column++)
                {
                    var cell = field.Cells[row, column];
                    if(cell.Value != CellValue.Mine)
                        field.DoOperation(new PointI(row, column), Field.Mode.Open);
                }
            }

            Assert.AreEqual(GameStatus.Won, field.GameStatus);
        }

        [TestMethod]
        public void FlagCellShouldFlagCellAndDecreaseMineLeftCounter()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            Assert.AreEqual(CellStatus.Flagged, field.Cells[0, 0].Status);
            Assert.AreEqual(_size.MinesTotal - 1, field.MinesLeft);
        }

        [TestMethod]
        public void FlagCellTwiceShouldRemoveFlagCell()
        {
            var field = new Field(_size);
            Assert.AreEqual(_size.MinesTotal, field.MinesLeft);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            Assert.AreEqual(CellStatus.Covered, field.Cells[0, 0].Status);
            Assert.AreEqual(_size.MinesTotal, field.MinesLeft);
        }

        [TestMethod]
        public void OpenNearbyOnCoveredCellShouldOpenCoveredCell()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), Field.Mode.OpenNearby);
            Assert.AreEqual(CellStatus.Opened, field.Cells[0, 0].Status);
        }

        [TestMethod]
        public void OpenNearbyOnNearbyFlagCountMatchesValueShouldOpenNearbyCovered()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            for(var row = 0; row < field.Cells.GetLength(0); row++)
            {
                for(var column = 0; column < field.Cells.GetLength(1); column++)
                {
                    var cell = field.Cells[row, column];
                    if(cell.Value > CellValue.Empty)
                    {
                        var nearbyCellPoints = field.GetNearbyCellPoints(new PointI(row, column)).ToList();
                        var isCoveredPoint = default(PointI);
                        foreach(var nearbyPoint in nearbyCellPoints)
                        {
                            var nearbyCell = field.Cells[nearbyPoint.X, nearbyPoint.Y];
                            if(nearbyCell.Value == CellValue.Mine && nearbyCell.Status != CellStatus.Flagged)
                                field.DoOperation(nearbyPoint, Field.Mode.Flag);
                            else if(nearbyCell.Status == CellStatus.Covered)
                                isCoveredPoint = nearbyPoint;
                        }
                        if(isCoveredPoint == null) //all open already
                            continue;

                        Assert.IsTrue(field.Cells[isCoveredPoint.X, isCoveredPoint.Y].Status == CellStatus.Covered);
                        field.DoOperation(new PointI(row, column), Field.Mode.OpenNearby);
                        Assert.IsTrue(field.Cells[isCoveredPoint.X, isCoveredPoint.Y].Status == CellStatus.Opened);
                        return;
                    }
                }
            }

            throw new Exception("Make sure to hit 'return' in the loop");
        }

        [TestMethod]
        public void UndoOnStartedGameShouldNotChangeCells()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            var changed = field.Undo();
            Assert.IsNull(changed);
        }

        [TestMethod]
        public void UndoOnGameOverShouldChangeCell()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), Field.Mode.Flag);
            for(var row = 0; row < field.Cells.GetLength(0); row++)
            {
                for(var column = 0; column < field.Cells.GetLength(1); column++)
                {
                    var cell = field.Cells[row, column];
                    if(cell.Value == CellValue.Mine)
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
        public void GetElapsedBeforeStartShouldBeZero()
        {
            var field = new Field(_size);
            Thread.Sleep(100);
            Assert.AreEqual(0, field.GetElapsedMilliseconds);
        }

        [TestMethod]
        public void GetElapsedAfterStartShouldBeBiggerThanZero()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), Field.Mode.Open);
            Thread.Sleep(100);
            Assert.IsTrue(field.GetElapsedMilliseconds > 0);
        }
    }
}
