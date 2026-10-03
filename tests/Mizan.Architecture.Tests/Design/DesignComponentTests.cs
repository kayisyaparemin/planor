using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// T3 bileşen kitaplığının mimari ve tasarım kurallarını (K3, K8, GK1, GK2, GK3, GK6, GK8)
/// denetleyen test kalkanı.
/// </summary>
public sealed class DesignComponentTests
{
    private static readonly string[] ExpectedComponents =
    [
        "PageHeader",
        "PeriodRail",
        "HeroInputCard",
        "SummaryCard",
        "ListCard",
        "ComparisonStrip",
        "MetricRow",
        "NavRow",
        "InfoBanner",
        "ChartCard",
        "StateBlock",
        "ReminderCard",
        "HeroPager",
        "EntryTypeTiles"
    ];

    [Fact]
    public void Bilesenler_SistemdekiBilesenler_Mevcut()
    {
        var componentsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components");
        Assert.True(Directory.Exists(componentsDir), "src/Mizan.App/Components dizini bulunamadı.");

        foreach (var comp in ExpectedComponents)
        {
            var xamlPath = Path.Combine(componentsDir, $"{comp}.xaml");
            var csPath = Path.Combine(componentsDir, $"{comp}.xaml.cs");

            Assert.True(File.Exists(xamlPath), $"{comp}.xaml dosyası bulunamadı.");
            Assert.True(File.Exists(csPath), $"{comp}.xaml.cs dosyası bulunamadı.");
        }
    }

    [Fact]
    public void Bilesenler_ContentView_Turevi()
    {
        var componentsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components");
        if (!Directory.Exists(componentsDir))
        {
            Assert.Fail("src/Mizan.App/Components dizini henüz mevcut değil.");
            return;
        }

        foreach (var comp in ExpectedComponents)
        {
            var xamlPath = Path.Combine(componentsDir, $"{comp}.xaml");
            var csPath = Path.Combine(componentsDir, $"{comp}.xaml.cs");

            if (!File.Exists(xamlPath) || !File.Exists(csPath))
            {
                Assert.Fail($"{comp} dosyaları eksik.");
                continue;
            }

            var xaml = File.ReadAllText(xamlPath);
            Assert.Contains("<ContentView", xaml, StringComparison.Ordinal);

            var cs = File.ReadAllText(csPath);
            Assert.Contains($": ContentView", cs, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Bilesenler_TurkceSummary_Tasar()
    {
        var componentsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components");
        if (!Directory.Exists(componentsDir))
        {
            Assert.Fail("src/Mizan.App/Components dizini henüz mevcut değil.");
            return;
        }

        foreach (var comp in ExpectedComponents)
        {
            var csPath = Path.Combine(componentsDir, $"{comp}.xaml.cs");
            if (!File.Exists(csPath)) { continue; }

            var cs = File.ReadAllText(csPath);
            Assert.Matches(new Regex(@"///\s*<summary>\s*\r?\n\s*///\s*[^<]+", RegexOptions.Multiline), cs);
        }
    }

    [Fact]
    public void Bilesenler_K3_SatirSinirlarini_Asamaz()
    {
        var componentsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components");
        if (!Directory.Exists(componentsDir)) { return; }

        var files = Directory.GetFiles(componentsDir, "*.*", SearchOption.AllDirectories);
        var failures = new List<string>();

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file).Length;
            if (lines > 200)
            {
                failures.Add($"{Path.GetFileName(file)}: {lines} satır (sınır 200)");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
}
