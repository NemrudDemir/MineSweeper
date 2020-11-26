using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
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
            minesCount.Should().Be(_size.MinesTotal);
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
            cells.Count(cell => cell.Value == CellValue.Mine).Should().Be(0);
            field.DoOperation(new PointI(0, 0), FieldMode.Open);
            cells.Count(cell => cell.Value == CellValue.Mine).Should().Be(_size.MinesTotal);
        }

        [TestMethod]
        public void OpenCellFirstOpenShouldOpenMoreThanOneCell()
        {
            var field = new Field(_size);
            var changedCells = field.DoOperation(new PointI(0, 0), FieldMode.Open);
            changedCells.Count.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void OpenCellMineOpenShouldBeGameOver()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), FieldMode.Flag);

            for(var row = 0; row < field.Cells.GetLength(0); row++)
            {
                for(var column = 0; column < field.Cells.GetLength(1); column++)
                {
                    var cell = field.Cells[row, column];
                    if(cell.Value == CellValue.Mine)
                    {
                        field.DoOperation(new PointI(row, column), FieldMode.Open);
                        field.GameStatus.Should().Be(GameStatus.Lost);
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
            field.DoOperation(new PointI(0, 0), FieldMode.Open);
            for(var row = 0; row < field.Cells.GetLength(0); row++)
            {
                for(var column = 0; column < field.Cells.GetLength(1); column++)
                {
                    var cell = field.Cells[row, column];
                    if(cell.Value != CellValue.Mine)
                        field.DoOperation(new PointI(row, column), FieldMode.Open);
                }
            }

            field.GameStatus.Should().Be(GameStatus.Won);
        }

        [TestMethod]
        public void FlagCellShouldFlagCellAndDecreaseMineLeftCounter()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), FieldMode.Flag);
            field.Cells[0, 0].Status.Should().Be(CellStatus.Flagged);
            field.MinesLeft.Should().Be(_size.MinesTotal - 1);
        }

        [TestMethod]
        public void FlagCellTwiceShouldRemoveFlagCell()
        {
            var field = new Field(_size);
            field.MinesLeft.Should().Be(_size.MinesTotal);
            field.DoOperation(new PointI(0, 0), FieldMode.Flag);
            field.DoOperation(new PointI(0, 0), FieldMode.Flag);
            field.Cells[0, 0].Status.Should().Be(CellStatus.Covered);
            field.MinesLeft.Should().Be(_size.MinesTotal);
        }

        [TestMethod]
        public void OpenNearbyOnCoveredCellShouldOpenCoveredCell()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), FieldMode.OpenNearby);
            field.Cells[0, 0].Status.Should().Be(CellStatus.Opened);
        }

        [TestMethod]
        public void UndoOnStartedGameShouldNotChangeCells()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), FieldMode.Open);
            var changed = field.Undo();
            changed.Should().BeNull();
        }

        [TestMethod]
        public void UndoOnGameOverShouldChangeCell()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), FieldMode.Flag);
            for(var row = 0; row < field.Cells.GetLength(0); row++)
            {
                for(var column = 0; column < field.Cells.GetLength(1); column++)
                {
                    var cell = field.Cells[row, column];
                    if(cell.Value == CellValue.Mine)
                    {
                        field.DoOperation(new PointI(row, column), FieldMode.Open);
                        field.GameStatus.Should().Be(GameStatus.Lost);
                        var changedCell = field.Undo();
                        changedCell.X.Should().Be(row);
                        changedCell.Y.Should().Be(column);
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
            field.GetElapsedMilliseconds.Should().Be(0);
        }

        [TestMethod]
        public void GetElapsedAfterStartShouldBeBiggerThanZero()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), FieldMode.Open);
            Thread.Sleep(100);
            field.GetElapsedMilliseconds.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void SetUserSettingsShouldSetProperty()
        {
            var field = new Field(_size);
            var settings = new UserSettings();
            field.UserSettings = settings;
            field.UserSettings.Should().Be(settings);
        }

        [TestMethod]
        public void DoOperationWithInvalidModeShouldReturnEmptyList()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), (FieldMode)(-1)).Should().BeEmpty();
        }

        [TestMethod]
        public void DoOperationWithOpenNearbyModeOnFlaggedCellShouldReturnEmptyList()
        {
            var field = new Field(_size);
            var point = new PointI(0, 0);
            field.DoOperation(point, FieldMode.Flag);
            field.DoOperation(point, FieldMode.OpenNearby).Should().BeEmpty();
        }

        [TestMethod]
        public void DoOperationOnNonRunningGameShouldReturnEmptyList()
        {
            var field = new Field(_size);
            field.DoOperation(new PointI(0, 0), FieldMode.Open);
            for(var x = 0; x<_size.X; x++)
                for(var y = 0; y<_size.Y; y++)
                    if(field.Cells[x, y].Value != CellValue.Mine)
                        field.DoOperation(new PointI(x, y), FieldMode.Open);

            field.GameStatus.Should().Be(GameStatus.Won);
            var minePoint = default(PointI);
            for(var x = 0; x < _size.X; x++)
                for(var y = 0; y < _size.Y; y++)
                    if(field.Cells[x, y].Value == CellValue.Mine)
                        minePoint = new PointI(x, y);

            field.DoOperation(minePoint, FieldMode.Open).Should().BeEmpty();
        }
    }
}
