using System.Reflection;
using System.Text.RegularExpressions;
using Mizan.Presentation.Automation;
using Xunit;

namespace Mizan.Presentation.Tests.Automation;

public sealed class AutomationIdsTests
{
    [Fact]
    public void TumKimlikler_KebabCaseKuralinaUyar()
    {
        var fields = typeof(AutomationIds).GetFields(BindingFlags.Public | BindingFlags.Static);
        var kebabRegex = new Regex(@"^[a-z]+(-[a-z0-9]+)+$");

        foreach (var field in fields)
        {
            var value = (string)field.GetValue(null)!;
            Assert.Matches(kebabRegex, value);
        }
    }

    [Fact]
    public void Kimlikler_MukerrerDegerIceremez()
    {
        var fields = typeof(AutomationIds).GetFields(BindingFlags.Public | BindingFlags.Static);
        var values = fields.Select(f => (string)f.GetValue(null)!).ToList();

        var duplicates = values.GroupBy(v => v).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.Empty(duplicates);
    }
}
