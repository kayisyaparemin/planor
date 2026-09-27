using Mizan.App.Pages;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App;

/// <summary>
/// Uygulamanın flyout menüsünü ve genel gezinme kabuğunu barındıran Shell sınıfı.
/// </summary>
public partial class AppShell : Microsoft.Maui.Controls.Shell
{
    /// <summary>
    /// <see cref="AppShell"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    /// <summary>
    /// <see cref="AppShell"/> sınıfının belirtilen ViewModel ile yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="viewModel">Kabuk görünüm modeli.</param>
    public AppShell(AppShellViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(Routes.ProfileSelection, typeof(ProfileSelectionPage));
        Routing.RegisterRoute(Routes.Onboarding, typeof(OnboardingPage));
        Routing.RegisterRoute(Routes.CardControl, typeof(CardControlPage));
        Routing.RegisterRoute(Routes.CardForm, typeof(CardFormPage));
        Routing.RegisterRoute(Routes.LoanForm, typeof(LoanFormPage));
    }
}
