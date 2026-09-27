using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Kesilmiş ekstrenin vadede nasıl ve ne kadar ödeneceğini, kartın borç ve limit durumunu
/// sunan kart kontrol sayfası (EK-V7). V6'dan <c>cardId</c> sorgu parametresiyle, V6 gelene
/// kadar sol menüdeki geçici öğeden açılır (S61).
/// </summary>
public partial class CardControlPage : ContentPage, IQueryAttributable
{
    private const string CardIdQueryKey = "cardId";
    private readonly CardControlViewModel _viewModel;
    private Guid? _requestedCardId;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public CardControlPage(CardControlViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen kart kimliğini bir sonraki yüklemeye saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _requestedCardId = query.TryGetValue(CardIdQueryKey, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
            ? id
            : null;
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(_requestedCardId);
            _requestedCardId = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CardControlPage yükleme hatası: {ex}");
        }
    }
}
