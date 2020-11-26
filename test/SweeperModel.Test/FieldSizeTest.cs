using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SweeperModel.Exceptions;
using System;
using System.Linq;

namespace SweeperModel.Test
{
    [TestClass]
    public class FieldSizeTest
    {
        private const int X = 20;
        private const int Y = 10;
        private const int MINES = 35;

        [TestMethod]
        public void CreateFieldSizeWithValidOptionsShouldNotThrow()
        {
            var size = new FieldSize(X, Y, MINES);

            size.X.Should().Be(X);
            size.Y.Should().Be(Y);
            size.MinesTotal.Should().Be(MINES);
        }

        [TestMethod]
        public void CreateFieldSizeWithValidOptionsMaxBoundShouldNotThrow()
        {
            var max = FieldSize.MAX_XY;
            var field = new FieldSize(max, max, FieldSize.GetMaxMines(max, max));
        }

        [TestMethod]
        public void CreateFieldSizeWithValidOptionsMinBoundShouldNotThrow()
        {
            var min = FieldSize.MIN_XY;
            var field = new FieldSize(min, min, FieldSize.GetMinMines());
        }

        [TestMethod]
        [DataRow(X, Y, -1)]
        [DataRow(0, Y, MINES)]
        [DataRow(X, 0, MINES)]
        public void CreateFieldSizeWithInvalidOptionsShouldThrow(int x, int y, int mines)
        {
            Action creatingFieldSize = () => new FieldSize(x, y, mines);
            creatingFieldSize.Should().Throw<InvalidFieldSizeException>();
        }

        [TestMethod]
        public void StandardsShouldReturnFields()
        {
            FieldSize.Standards.Count().Should().BePositive();
        }

        [TestMethod]
        public void ToStringShouldNotBeNullOrEmpty()
        {
            new FieldSize(X, Y, MINES).ToString().Should().NotBeNullOrEmpty();
        }
    }
}
