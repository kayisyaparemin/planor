using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Kesilmiş ekstrenin vadede nasıl ve ne kadar ödeneceğini, kartın borç ve limit durumunu
/// sunan kart kontrol sayfası (EK-V7). Finansal Yapı'daki kart satırından
/// <see cref="Routes.CardIdParameter"/> ile açılır (S61-8, S62-9).
/// </summary>
public partial class CardControlPage : ContentPage, IQueryAttributable
{
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
        _requestedCardId = query.TryGetValue(Routes.CardIdParameter, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
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
