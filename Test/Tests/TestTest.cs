using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.Tests
{
    [TestFixture]
    public class TestTest : AvaloniaTest
    {
        [Test]
        public async Task TestTestTestOne() // dont @ me
        {
            var executed = false;
            await Post(() =>
            {
                Assert.DoesNotThrow(() => Assert.NotNull(App));
                executed = true;
            });
            
            Assert.That(executed, Is.True);
        }

        [Test]
        public async Task TestTestTestTwo()
        {
            var executed = false;
            await Post(() =>
            {
                Assert.DoesNotThrow(() => Assert.NotNull(App));
                executed = true;
            });
            
            Assert.That(executed, Is.True);
        }
    }
}