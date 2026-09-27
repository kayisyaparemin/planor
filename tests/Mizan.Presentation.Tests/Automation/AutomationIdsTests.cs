using System.Reflection;
using System.Text.RegularExpressions;
using Mizan.Presentation.Automation;
using Xunit;

namespace Mizan.Presentation.Tests.Automation;

/// <summary>
/// Otomasyon kimliklerinin biçimi ve tekilliği. Kimlikler dosya sınırı yüzünden
/// <see cref="AutomationIds"/> ve aynı ad alanındaki diğer statik sınıflara (örn.
/// <see cref="RecordFormAutomationIds"/>) bölünür; kurallar hepsine birlikte uygulanır.
/// </summary>
public sealed class AutomationIdsTests
{
    [Fact]
    public void TumKimlikler_KebabCaseKuralinaUyar()
    {
        var kebabRegex = new Regex(@"^[a-z]+(-[a-z0-9]+)+$");

        foreach (var value in AllIds())
        {
            Assert.Matches(kebabRegex, value);
        }
    }

    [Fact]
    public void Kimlikler_MukerrerDegerIceremez()
    {
        var duplicates = AllIds().GroupBy(v => v).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void KimlikSiniflari_AyniAdAlanindaBulunur()
    {
        Assert.Contains(typeof(RecordFormAutomationIds), IdClasses());
        Assert.Contains(typeof(AutomationIds), IdClasses());
    }

    private static IEnumerable<Type> IdClasses() =>
        typeof(AutomationIds).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(AutomationIds).Namespace && t.IsAbstract && t.IsSealed);

    private static List<string> AllIds() =>
        IdClasses()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Select(f => (string)f.GetValue(null)!)
            .ToList();
}
