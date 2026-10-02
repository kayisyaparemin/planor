using Microsoft.Extensions.Logging;
using Mizan.App.Platforms.Android;
using Mizan.App.Services;
using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Calculations;
using Mizan.Infrastructure.Backup;
using Mizan.Infrastructure.Persistence;
using Mizan.Infrastructure.Persistence.Repositories;
using Mizan.Infrastructure.Telemetry;
using Mizan.Infrastructure.Time;
using Mizan.App.Pages;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Presenters;
using Mizan.Presentation.Services;
using Mizan.Presentation.ViewModels;
using SQLite;

namespace Mizan.App;

/// <summary>Uygulamanın kompozisyon kökünü kuran, bağımlılıkları kaydeden ve MAUI uygulamasını başlatan sınıf.</summary>
public static class MauiProgram
{
    /// <summary>MAUI uygulamasını ve servis grafiğini inşa eder.</summary>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialSymbolsRounded.ttf", "MaterialSymbols");
            });

        RegisterDomainServices(builder.Services);
        RegisterInfrastructureServices(builder.Services);
        RegisterApplicationServices(builder.Services);
        RegisterPresentationAndAppServices(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();

        // Eksik bir kayıt, sayfa ilk açıldığında değil açılışta ve eksiklerin tamamıyla birlikte patlasın.
        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions { ValidateOnBuild = true }));
#endif

        return builder.Build();
    }

    private static void RegisterDomainServices(IServiceCollection services)
    {
        services.AddSingleton<CashFlowPeriodCalculator>();
        services.AddSingleton<IncomeResolver>();
        services.AddSingleton<IncomeProjectionCalculator>();
        services.AddSingleton<LoanScheduleCalculator>();
        services.AddSingleton<LoanAmortizationCalculator>();
        services.AddSingleton<LoanPaymentScheduleBuilder>();
        services.AddSingleton<LoanPrepaymentValidator>();
        services.AddSingleton<InstallmentScheduleCalculator>();
        services.AddSingleton<ScheduledPaymentCalculator>();
        services.AddSingleton<CreditCardStatementCalculator>();
        services.AddSingleton<CreditCardPaymentPreferenceResolver>();
        services.AddSingleton<CreditCardActualPaymentReconciler>();
        services.AddSingleton<LoanInstrumentReconciler>();
        services.AddSingleton<MandatoryPaymentCalculator>();
        services.AddSingleton<PeriodObligationGrouper>();
        services.AddSingleton<FinancialProjectionCalculator>();
        services.AddSingleton<TargetAmountCalculator>();
        services.AddSingleton<SimulationCalculator>();
        services.AddSingleton<ScenarioPlanBuilder>();
    }

    private static void RegisterInfrastructureServices(IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ITelemetryService, NullTelemetryService>();

        var profileRepository = new FileSystemProfileRepository(FileSystem.AppDataDirectory);
        services.AddSingleton<IProfileRepository>(profileRepository);
        services.AddSingleton<IProfileFileLayout>(profileRepository);

        services.AddSingleton(new DatabaseSchema());
        services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
        services.AddSingleton<SqliteProfileStoreSwitch>();
        services.AddSingleton<IProfileStoreSwitch>(sp => sp.GetRequiredService<SqliteProfileStoreSwitch>());
        services.AddSingleton<ISqliteConnectionProvider>(sp => sp.GetRequiredService<SqliteProfileStoreSwitch>());
        services.AddTransient<SQLiteAsyncConnection>(sp => sp.GetRequiredService<ISqliteConnectionProvider>().Connection);

        services.AddSingleton<IProfileBackupArchive, ProfileBackupArchive>();
        services.AddSingleton<IStorageAccess, AndroidStorageAccess>();
        services.AddSingleton<IBackupStorage>(new FolderBackupStorage(
            AndroidStorageAccess.FolderPath,
            $"Dahili depolama › {AndroidStorageAccess.FolderName}",
            new AndroidStorageAccess()));
        services.AddSingleton(new BackupOptions(Path.Combine(FileSystem.CacheDirectory, "backup")));
        services.AddSingleton<IBackupService, BackupService>();

        RegisterRepositories(services);
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddTransient<IUserSettingsRepository, SqliteUserSettingsRepository>();
        services.AddTransient<IRecurringIncomeRepository, SqliteRecurringIncomeRepository>();
        services.AddTransient<IAdHocIncomeRepository, SqliteAdHocIncomeRepository>();
        services.AddTransient<ILoanRepository, SqliteLoanRepository>();
        services.AddTransient<ITemporaryPaymentPlanRepository, SqliteTemporaryPaymentPlanRepository>();
        services.AddTransient<IPlannedLargeExpenseRepository, SqlitePlannedLargeExpenseRepository>();
        services.AddTransient<ICreditCardRepository, SqliteCreditCardRepository>();
        services.AddTransient<ISimulationDraftRepository, SqliteSimulationDraftRepository>();
        services.AddTransient<IPaymentReminderRepository, SqlitePaymentReminderRepository>();
        services.AddTransient<IPeriodObservationRepository, SqlitePeriodObservationRepository>();
        services.AddTransient<IPeriodHistoryRepository, SqlitePeriodHistoryRepository>();
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        services.AddSingleton<ProfileService>();
        services.AddSingleton<FinancialProjectionService>();
        services.AddSingleton<ProjectionBoundaryResolver>();
        services.AddSingleton<PeriodPlanSnapshotService>();
        services.AddSingleton<FinancialSnapshotService>();
        services.AddSingleton<HistoricalPlanRevisionService>();
        services.AddSingleton<FinancialInstrumentReconciliationService>();
        services.AddSingleton<PlanActualComparisonCalculator>();
        services.AddSingleton<LoanPayoffService>();
        services.AddSingleton<LoanPayoffAdvisor>();
        services.AddSingleton<HistoryQueryService>();
        services.AddSingleton<OpenPeriodLedgerReader>();
        services.AddSingleton<PeriodProgressService>();
        services.AddSingleton<IPeriodProgressService>(sp => sp.GetRequiredService<PeriodProgressService>());
        services.AddSingleton<PeriodSettlementService>();
        services.AddSingleton<IncomePlanReader>();
        services.AddSingleton<IncomePlanWriter>();
        services.AddSingleton<FinancialInstrumentReader>();
        services.AddSingleton<FinancialInstrumentWriter>();
        services.AddSingleton<IPlanReader, PlanReader>();
        services.AddSingleton<IPlanChangeRecorder, PlanChangeRecorder>();
        services.AddSingleton<ICreditCardObligationService, CreditCardObligationService>();
        services.AddSingleton<ISimulationPlanApplier, SimulationPlanApplier>();
        services.AddSingleton<ISimulationWorkflowService, SimulationWorkflowService>();
        services.AddSingleton<IObligationManagementService, ObligationManagementService>();
        services.AddSingleton<IPeriodWorkflowService, PeriodWorkflowService>();
        services.AddSingleton<IPaymentReminderService, PaymentReminderService>();
        services.AddSingleton<IIncomePlanService, IncomePlanService>();
        services.AddSingleton<OnboardingPlanWriter>();
        services.AddSingleton<IOnboardingService, OnboardingService>();
    }

    private static void RegisterPresentationAndAppServices(IServiceCollection services)
    {
        services.AddSingleton<CashFlowPeriodDetailPresenter>();
        services.AddSingleton<SimulatorInsightService>();

        services.AddSingleton<INavigationService, MauiNavigationService>();
        services.AddSingleton<IDialogService, MauiDialogService>();

        services.AddSingleton<IBackupFilePicker, AndroidBackupFilePicker>();
        services.AddSingleton<IProfileBackupHandler, ProfileBackupHandler>();
        services.AddSingleton<IPaymentReminderScheduler, InMemoryPaymentReminderScheduler>();
        RegisterScreens(services);
    }

    private static void RegisterScreens(IServiceCollection services)
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
        services.AddTransient<CardControlViewModel>();
        services.AddTransient<CardControlPage>();
        services.AddTransient<FinancialRecordRowBuilder>();
        services.AddTransient<FinancialRecordRemover>();
        services.AddTransient<FinancialStructureViewModel>();
        services.AddTransient<FinancialStructurePage>();
        services.AddTransient<CardFormViewModel>();
        services.AddTransient<CardFormPage>();
        services.AddTransient<LoanFormViewModel>();
        services.AddTransient<LoanFormPage>();
        services.AddTransient<IncomeFormViewModel>();
        services.AddTransient<IncomeFormPage>();
        services.AddTransient<AdHocIncomeFormViewModel>();
        services.AddTransient<AdHocIncomeFormPage>();
    }
}
