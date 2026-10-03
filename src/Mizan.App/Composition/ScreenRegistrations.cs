using Mizan.App.Pages;
using Mizan.Presentation.Services;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Composition;

/// <summary>
/// Kompozisyon kökünün ekran yarısı: her sayfa ve görünüm modeli burada kaydedilir. <c>MauiProgram.cs</c> 200 satır
/// sınırına dayandığı için ayrıldı (V9, kullanıcı kararı); kayıtlar yine tek yerde okunur ve
/// <c>ArchitectureTests.DiKaydi_YalnizKompozisyonKokundeOlabilir</c> kaydın bu iki dosyanın dışına çıkmasını engeller.
/// Sayfalar geçicidir: her açılışta yeni sayfa, yeni görünüm modeli.
/// </summary>
public static class ScreenRegistrations
{
    /// <summary>Kabuğu, sayfaları ve görünüm modellerini kaydeder.</summary>
    public static IServiceCollection AddScreens(this IServiceCollection services)
    {
        services.AddTransient<AppShellViewModel>();
        services.AddTransient<AppShell>();
        services.AddTransient<ProfileSelectionViewModel>();
        services.AddTransient<ProfileSelectionPage>();
        services.AddTransient<ReminderCardViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<BalanceEntryViewModel>();
        services.AddTransient<BalanceEntryPage>();
        services.AddTransient<OnboardingViewModel>();
        services.AddTransient<OnboardingPage>();
        AddFinancialStructureScreens(services);
        services.AddTransient<FuturePeriodsViewModel>();
        services.AddTransient<FuturePeriodsPage>();
        services.AddTransient<PeriodDetailViewModel>();
        services.AddTransient<PeriodDetailPage>();
        services.AddTransient<SimulationResultViewModel>();
        services.AddTransient<SimulatorViewModel>();
        services.AddTransient<SimulatorPage>();
        services.AddTransient<SimulationConditionPickerViewModel>();
        services.AddTransient<SimulationConditionPickerPage>();
        services.AddTransient<SimulationConditionViewModel>();
        services.AddTransient<SimulationConditionPage>();
        return services;
    }

    // Finansal Yapı listesi, formları ve kart kontrol (V6, V7).
    private static void AddFinancialStructureScreens(IServiceCollection services)
    {
        services.AddTransient<CardControlViewModel>();
        services.AddTransient<CardControlPage>();
        services.AddTransient<FinancialRecordRowBuilder>();
        services.AddTransient<RecordCandidateResolver>();
        services.AddTransient<FinancialRecordRemover>();
        services.AddTransient<FinancialStructureViewModel>();
        services.AddTransient<FinancialStructurePage>();
        services.AddTransient<RecordEntryPickerViewModel>();
        services.AddTransient<RecordEntryPickerPage>();
        services.AddTransient<CardFormViewModel>();
        services.AddTransient<CardFormPage>();
        services.AddTransient<LoanFormViewModel>();
        services.AddTransient<LoanFormPage>();
        services.AddTransient<IncomeFormViewModel>();
        services.AddTransient<IncomeFormPage>();
        services.AddTransient<AdHocIncomeFormViewModel>();
        services.AddTransient<AdHocIncomeFormPage>();
        services.AddTransient<PaymentPlanFormViewModel>();
        services.AddTransient<PaymentPlanFormPage>();
        services.AddTransient<PlannedExpenseFormViewModel>();
        services.AddTransient<PlannedExpenseFormPage>();
    }
}
