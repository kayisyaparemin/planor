using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Bankadaki bakiyeyi dönemin bir gününe yazan ve kaydetmeden önce bu girişle dönem sonunu gösteren sayfa
/// (EK-V3 sayfa 2). Ana sayfadaki "Bakiye Gir"den açılır, ✕ ile onaysız kapanır.
/// </summary>
public partial class BalanceEntryPage : ContentPage
{
    private readonly BalanceEntryViewModel _viewModel;
    private bool _loaded;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public BalanceEntryPage(BalanceEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
        {
            return; // Tarih penceresinden ya da uyarıdan dönüşte girilen tutar silinmesin.
        }

        _loaded = true;
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BalanceEntryPage yükleme hatası: {ex}");
        }
    }
}
