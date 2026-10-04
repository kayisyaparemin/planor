using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Kapanmış nakit akış dönemlerinin tarihçesini ve son dönemlerin net değişim
/// özetini gösteren sayfa (EK-V12, S29, GS32).
/// </summary>
public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    /// <summary>Geçmiş sayfasını görünüm modeliyle başlatır.</summary>
    public HistoryPage(HistoryViewModel viewModel)
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
            System.Diagnostics.Debug.WriteLine($"HistoryPage yükleme hatası: {ex}");
        }
    }
}
