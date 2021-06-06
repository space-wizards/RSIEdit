using Importer.RSI;
using NUnit.Framework;

namespace Importer.Tests.RSI.State
{
    [TestFixture]
    [TestOf(typeof(RsiState))]
    public class RsiStateTests
    {
        [Test]
        [TestCase(1, 1, 1)]
        [TestCase(2, 2, 1)]
        [TestCase(3, 2, 2)]
        [TestCase(4, 2, 2)]
        [TestCase(5, 3, 2)]
        [TestCase(6, 3, 2)]
        [TestCase(7, 3, 3)]
        [TestCase(8, 3, 3)]
        [TestCase(9, 3, 3)]
        [TestCase(10, 4, 3)]
        [TestCase(11, 4, 3)]
        [TestCase(12, 4, 3)]
        [TestCase(13, 4, 4)]
        [TestCase(14, 4, 4)]
        [TestCase(15, 4, 4)]
        [TestCase(16, 4, 4)]
        [TestCase(17, 5, 4)]
        [TestCase(18, 5, 4)]
        [TestCase(19, 5, 4)]
        [TestCase(20, 5, 4)]
        [TestCase(21, 5, 5)]
        [TestCase(22, 5, 5)]
        [TestCase(23, 5, 5)]
        [TestCase(24, 5, 5)]
        [TestCase(25, 5, 5)]
        [TestCase(26, 6, 5)]
        [TestCase(26, 6, 5)]
        [TestCase(1500, 39, 39)]
        public void GetRowsAndColumnsTest(int images, int expectedRows, int expectedColumns)
        {
            var (rows, columns) = RsiState.GetRowsAndColumns(images);

            Assert.That(rows, Is.EqualTo(expectedRows));
            Assert.That(columns, Is.EqualTo(expectedColumns));
        }
    }
}
