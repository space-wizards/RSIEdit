using System.Threading.Tasks;
using NUnit.Framework;

namespace Test.Tests;

[TestFixture]
public class JulianTests : AvaloniaTest
{
    [Test]
    public async Task NewRsi()
    {
        await Post(async () =>
        {
            await Vm.New();
            Assert.That(Vm.CurrentOpenRsi, Is.Not.Null);
            Assert.That(Vm.OpenRsis.Count, Is.EqualTo(1));
        });
    }
}