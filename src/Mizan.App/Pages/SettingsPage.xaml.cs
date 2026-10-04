using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Dönem çapa günü, yaşam gideri havuzu, varsayılan faiz oranları, bildirim modu
/// ve yerel yedekleme ayarlarını yöneten sayfa (EK-V13, GS33, S4, S18, S19, S79).
/// </summary>
public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    /// <summary>Ayarlar sayfasını görünüm modeliyle başlatır.</summary>
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SettingsPage yükleme hatası: {ex}");
        }
    }
}
