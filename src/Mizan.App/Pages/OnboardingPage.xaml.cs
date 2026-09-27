using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Sekiz adımlı kurulum sihirbazı sayfası.
/// </summary>
public partial class OnboardingPage : ContentPage
{
    /// <summary>
    /// <see cref="OnboardingPage"/> sınıfını ViewModel bağımlılığıyla başlatır.
    /// </summary>
    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
