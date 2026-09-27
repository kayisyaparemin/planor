using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Plana giren gelir, kart, kredi ve ödemeleri dört grupta listeleyen Finansal Yapı sayfası
/// (EK-V6). Kart kontrolden ya da silmeden dönüldüğünde liste her görünüşte yenilenir.
/// </summary>
public partial class FinancialStructurePage : ContentPage
{
    private readonly FinancialStructureViewModel _viewModel;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public FinancialStructurePage(FinancialStructureViewModel viewModel)
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
            System.Diagnostics.Debug.WriteLine($"FinancialStructurePage yükleme hatası: {ex}");
        }
    }
}
