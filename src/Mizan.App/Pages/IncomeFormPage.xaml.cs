using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Düzenli geliri ekleyen ya da düzenleyen gelir formu sayfası (EK-V6d). Finansal Yapı'dan
/// kimliksiz (yeni gelir) ya da <see cref="Routes.IncomeIdParameter"/> ile (düzenleme) açılır.
/// </summary>
public partial class IncomeFormPage : ContentPage, IQueryAttributable
{
    private readonly IncomeFormViewModel _viewModel;
    private Guid? _incomeId;
    private bool _loaded;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public IncomeFormPage(IncomeFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen gelir kimliğini yüklemeye saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _incomeId = query.TryGetValue(Routes.IncomeIdParameter, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
            ? id
            : null;
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
        {
            return; // Diyalogdan dönüşte form yeniden yüklenip girilenler silinmesin.
        }

        _loaded = true;
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(_incomeId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"IncomeFormPage yükleme hatası: {ex}");
        }
    }

    /// <summary>Cihazın geri tuşu da Vazgeç'ten geçer: değişiklik varsa onay sorulur.</summary>
    protected override bool OnBackButtonPressed()
    {
        if (_viewModel.CancelCommand.CanExecute(null))
        {
            _viewModel.CancelCommand.Execute(null);
        }

        return true;
    }
}
