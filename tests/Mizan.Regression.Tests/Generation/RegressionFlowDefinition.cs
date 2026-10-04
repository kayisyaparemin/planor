using Mizan.Presentation.Automation;

namespace Mizan.Regression.Tests.Generation;

/// <summary>
/// Planör E2E regresyon akışını C# AutomationIds sabitlerine doğrudan referans vererek kuran tanım.
/// </summary>
public static class RegressionFlowDefinition
{
    private const string PackageId = "com.mizan.app";

    /// <summary>
    /// Tüm regresyon senaryolarını içeren akışı oluşturur.
    /// </summary>
    public static MaestroFlowBuilder Build()
    {
        var builder = new MaestroFlowBuilder();
        builder.LaunchApp(PackageId, clearState: true);

        AppendScenario1To3(builder);
        AppendScenario4To6(builder);
        AppendScenario7To10(builder);

        return builder;
    }

    private static void AppendScenario1To3(MaestroFlowBuilder builder)
    {
        builder.Section("SENARYO 1: Profil Seçimi ve Temiz Başlangıç")
            .AssertVisibleId(AutomationIds.PageProfileSelection)
            .TapOnId(AutomationIds.BtnCleanStart)
            .InputText("RegresyonTesti")
            .TapOnText("Oluştur");

        builder.Section("SENARYO 2: Onboarding (Kurulum Sihirbazı)")
            .AssertVisibleId(AutomationIds.PageOnboarding);

        for (var i = 0; i < 7; i++)
        {
            builder.TapOnId(AutomationIds.BtnOnboardingNext);
        }

        builder.TapOnId(AutomationIds.BtnOnboardingStart)
            .AssertVisibleId(AutomationIds.PageDashboard);

        builder.Section("SENARYO 3: Dashboard ve Bakiye Gözlemi")
            .AssertVisibleId(AutomationIds.CardDashboardHero)
            .TapOnId(AutomationIds.BtnBalanceEntry)
            .AssertVisibleId(AutomationIds.PageBalanceEntry)
            .TapOnId(AutomationIds.InputBalanceAmount)
            .InputText("50000")
            .TapOnId(AutomationIds.BtnSaveBalance)
            .AssertVisibleId(AutomationIds.PageDashboard)
            .AssertVisibleId(AutomationIds.LblProjectedEnding);
    }

    private static void AppendScenario4To6(MaestroFlowBuilder builder)
    {
        builder.Section("SENARYO 4: 12 Dönem Projeksiyonu")
            .TapOnText("Open navigation drawer")
            .TapOnId(AutomationIds.FlyoutItemProjection)
            .AssertVisibleId(FuturePeriodsAutomationIds.PageFuturePeriods)
            .AssertVisibleId(FuturePeriodsAutomationIds.CardFutureTrend);

        builder.Section("SENARYO 5: Dönem Detayı Akışı")
            .TapOnId(FuturePeriodsAutomationIds.GridFuturePeriods)
            .AssertVisibleId(PeriodDetailAutomationIds.PagePeriodDetail)
            .Back()
            .AssertVisibleId(FuturePeriodsAutomationIds.PageFuturePeriods);

        builder.Section("SENARYO 6: Finansal Yapı")
            .TapOnText("Open navigation drawer")
            .TapOnId(AutomationIds.FlyoutItemFinancialStructure)
            .AssertVisibleId(AutomationIds.PageFinancialStructure)
            .TapOnId(AutomationIds.BtnStructureAdd)
            .AssertVisibleId(RecordEntryPickerAutomationIds.PageRecordEntryPicker)
            .Back()
            .AssertVisibleId(AutomationIds.PageFinancialStructure);
    }

    private static void AppendScenario7To10(MaestroFlowBuilder builder)
    {
        builder.Section("SENARYO 7: Simülatör")
            .TapOnText("Open navigation drawer")
            .TapOnId(AutomationIds.FlyoutItemSimulation)
            .AssertVisibleId(SimulatorAutomationIds.PageSimulator)
            .AssertVisibleId(SimulatorAutomationIds.BtnSimulatorAdd);

        builder.Section("SENARYO 8: Geçmiş (Tarihçe)")
            .TapOnText("Open navigation drawer")
            .TapOnId(AutomationIds.FlyoutItemHistory)
            .AssertVisibleId(HistoryAutomationIds.PageHistory);

        builder.Section("SENARYO 9: Ayarlar")
            .TapOnText("Open navigation drawer")
            .TapOnId(AutomationIds.FlyoutItemSettings)
            .AssertVisibleId(SettingsAutomationIds.Page)
            .AssertVisibleId(SettingsAutomationIds.BtnSave);

        builder.Section("SENARYO 10: Profil Değiştirme")
            .TapOnText("Open navigation drawer")
            .TapOnId(AutomationIds.FlyoutItemSwitchProfile)
            .AssertVisibleId(AutomationIds.PageProfileSelection);
    }
}
