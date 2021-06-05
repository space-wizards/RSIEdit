using Importer.Directions;
using Importer.RSI;
using NUnit.Framework;

namespace Importer.Tests.RSI.State
{
    [TestFixture]
    [TestOf(typeof(RsiState))]
    public class RsiStateTests
    {
        [Test]
        // 32x32 image
        [TestCase(32, 32, 0, 32, 32, 0, 0)]
        [TestCase(32, 32, 1, 32, 32, null, null)]
        [TestCase(32, 32, 2, 32, 32, null, null)]
        [TestCase(32, 32, 3, 32, 32, null, null)]

        // 64x32 image
        [TestCase(32, 32, 0, 64, 32, 0, 0)]
        [TestCase(32, 32, 1, 64, 32, 32, 0)]
        [TestCase(32, 32, 2, 64, 32, null, null)]
        [TestCase(32, 32, 3, 64, 32, null, null)]
        [TestCase(32, 32, 4, 64, 32, null, null)]
        [TestCase(32, 32, 5, 64, 32, null, null)]

        // 64x64 image
        [TestCase(32, 32, 0, 64, 64, 0, 0)]
        [TestCase(32, 32, 1, 64, 64, 32, 0)]
        [TestCase(32, 32, 2, 64, 64, 0, 32)]
        [TestCase(32, 32, 3, 64, 64, 32, 32)]
        [TestCase(32, 32, 4, 64, 64, null, null)]
        [TestCase(32, 32, 5, 64, 64, null, null)]
        public void FirstFrameTest(
            int sizeX,
            int sizeY,
            int direction,
            int fileWidth,
            int fileHeight,
            int? expectedX,
            int? expectedY)
        {
            var size = new RsiSize(sizeX, sizeY);
            var directionEnum = (Direction) direction;
            var state = new RsiState(string.Empty);

            var firstFrame = state.FirstFrameFor(size, directionEnum, fileWidth, fileHeight);

            if (!expectedX.HasValue)
            {
                Assert.Null(firstFrame);
                return;
            }

            Assert.NotNull(firstFrame);
            Assert.That(firstFrame.Value.X, Is.EqualTo(expectedX.Value));
            Assert.That(firstFrame.Value.Y, Is.EqualTo(expectedY!.Value));
            Assert.That(firstFrame.Value.Width, Is.EqualTo(size.X));
            Assert.That(firstFrame.Value.Height, Is.EqualTo(size.Y));
        }
    }
}
