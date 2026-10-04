using System.Reflection;
using Mizan.Presentation.Automation;

namespace Mizan.Regression.Tests.Generation;

/// <summary>
/// Maestro E2E akışının AutomationIds sabitlerinden doğru üretildiğini ve diskteki
/// dosya ile senkron kaldığını denetleyen koruyucu kalkan testleri.
/// </summary>
public sealed class MaestroFlowGeneratorTests
{
    [Fact]
    public void DisktekiYaml_GeneratorCiktisiylaBirebirAynidir()
    {
        var path = RegressionPaths.MaestroFlowPath;
        Assert.True(File.Exists(path), $"Maestro akış dosyası bulunamadı: {path}");

        var diskContent = File.ReadAllText(path).Replace("\r\n", "\n").TrimEnd();
        var generatedContent = MaestroFlowGenerator.GenerateYaml().Replace("\r\n", "\n").TrimEnd();

        Assert.Equal(generatedContent, diskContent);
    }

    [Fact]
    public void AkistaKullanilanTumIdler_AutomationIdsSiniflarindaMevcuttur()
    {
        var builder = RegressionFlowDefinition.Build();
        var knownIds = GetAllDefinedAutomationIds();

        foreach (var id in builder.ReferencedIds)
        {
            Assert.Contains(id, knownIds);
        }
    }

    [Fact]
    public void Akis_TumAnaEkranlari_Kapsar()
    {
        var builder = RegressionFlowDefinition.Build();
        var ids = builder.ReferencedIds.ToHashSet();

        Assert.Contains(AutomationIds.PageProfileSelection, ids);
        Assert.Contains(AutomationIds.PageOnboarding, ids);
        Assert.Contains(AutomationIds.PageDashboard, ids);
        Assert.Contains(FuturePeriodsAutomationIds.PageFuturePeriods, ids);
        Assert.Contains(PeriodDetailAutomationIds.PagePeriodDetail, ids);
        Assert.Contains(AutomationIds.PageFinancialStructure, ids);
        Assert.Contains(SimulatorAutomationIds.PageSimulator, ids);
        Assert.Contains(HistoryAutomationIds.PageHistory, ids);
        Assert.Contains(SettingsAutomationIds.Page, ids);
    }

    private static HashSet<string> GetAllDefinedAutomationIds()
    {
        var idClasses = typeof(AutomationIds).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(AutomationIds).Namespace && t.IsAbstract && t.IsSealed);

        return idClasses
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToHashSet();
    }
}
