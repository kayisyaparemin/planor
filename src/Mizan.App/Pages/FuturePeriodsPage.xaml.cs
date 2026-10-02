using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Ana sayfanın dönem sonundan başlayan 12 dönemde bakiyenin en çok nereye indiğini, yolunu, bir yıl sonrasını,
/// faizini ve her dönemin sonunu gösteren sayfa (EK-V8). Plan başka sayfada değişebildiği için her görünüşte
/// yeniden yüklenir.
/// </summary>
public partial class FuturePeriodsPage : ContentPage
{
    private readonly FuturePeriodsViewModel _viewModel;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public FuturePeriodsPage(FuturePeriodsViewModel viewModel)
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
            System.Diagnostics.Debug.WriteLine($"FuturePeriodsPage yükleme hatası: {ex}");
        }
    }
}
