using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Simülatör sayfası (EK-V10): kullanıcının "şunu yaparsam ne olur?" diye kurduğu denemelerin listesi. Deneme
/// formu başka sayfada yazdığı için her görünüşte yeniden yüklenir.
/// </summary>
public partial class SimulatorPage : ContentPage
{
    private readonly SimulatorViewModel _viewModel;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public SimulatorPage(SimulatorViewModel viewModel)
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
            System.Diagnostics.Debug.WriteLine($"SimulatorPage yükleme hatası: {ex}");
        }
    }
}
