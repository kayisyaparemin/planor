using System.Reflection;
using Mizan.Presentation.Navigation;
using Xunit;

namespace Mizan.Presentation.Tests.Navigation;

public sealed class RoutesTests
{
    [Fact]
    public void KokRotalar_CiftBolukleBaslar()
    {
        Assert.StartsWith("//", Routes.Dashboard);
        Assert.StartsWith("//", Routes.Projection);
        Assert.StartsWith("//", Routes.Simulation);
        Assert.StartsWith("//", Routes.Commitments);
        Assert.StartsWith("//", Routes.History);
        Assert.StartsWith("//", Routes.Settings);
    }

    [Fact]
    public void Rotalar_YasakliTerimleriIcermez()
    {
        var fields = typeof(Routes).GetFields(BindingFlags.Public | BindingFlags.Static);
        var forbiddenWords = new[] { "salary", "review", "savings", "coinflow" };

        foreach (var field in fields)
        {
            var value = (string)field.GetValue(null)!;
            foreach (var forbidden in forbiddenWords)
            {
                Assert.DoesNotContain(forbidden, value, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(forbidden, field.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
