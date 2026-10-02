using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Tek seferlik arızi geliri ekleyen ya da düzenleyen form sayfası (EK-V6d). Finansal Yapı'dan
/// kimliksiz (yeni gelir) ya da <see cref="Routes.AdHocIncomeIdParameter"/> ile (düzenleme) açılır.
/// </summary>
public partial class AdHocIncomeFormPage : ContentPage, IQueryAttributable
{
    private readonly AdHocIncomeFormViewModel _viewModel;
    private Guid? _adHocIncomeId;
    private bool _loaded;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public AdHocIncomeFormPage(AdHocIncomeFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen gelir kimliğini yüklemeye saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _adHocIncomeId = query.TryGetValue(Routes.AdHocIncomeIdParameter, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
            ? id
            : null;
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(_adHocIncomeId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AdHocIncomeFormPage yükleme hatası: {ex}");
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
