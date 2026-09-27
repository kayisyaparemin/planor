using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Kartın tanımını ekleyen ya da düzenleyen kart formu sayfası (EK-V6b). Finansal Yapı'dan
/// kimliksiz (yeni kart) ya da <see cref="Routes.CardIdParameter"/> ile (düzenleme) açılır.
/// </summary>
public partial class CardFormPage : ContentPage, IQueryAttributable
{
    private readonly CardFormViewModel _viewModel;
    private Guid? _cardId;
    private bool _loaded;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public CardFormPage(CardFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen kart kimliğini yüklemeye saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _cardId = query.TryGetValue(Routes.CardIdParameter, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
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
            await _viewModel.LoadCommand.ExecuteAsync(_cardId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CardFormPage yükleme hatası: {ex}");
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
