using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Kredinin tanımını ekleyen ya da düzenleyen kredi formu sayfası (EK-V6c). Finansal Yapı'dan
/// kimliksiz (yeni kredi) ya da <see cref="Routes.LoanIdParameter"/> ile (düzenleme) açılır.
/// </summary>
public partial class LoanFormPage : ContentPage, IQueryAttributable
{
    private readonly LoanFormViewModel _viewModel;
    private Guid? _loanId;
    private bool _loaded;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public LoanFormPage(LoanFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen kredi kimliğini yüklemeye saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _loanId = query.TryGetValue(Routes.LoanIdParameter, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
            ? id
            : null;
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
        {
            return; // Diyalogdan ya da tarih seçiciden dönüşte form yeniden yüklenip girilenler silinmesin.
        }

        _loaded = true;
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(_loanId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoanFormPage yükleme hatası: {ex}");
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
