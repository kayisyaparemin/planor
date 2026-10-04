using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Kapanmış bir dönemin dondurulmuş planına göre gerçekleşen kapanış bakiyesini,
/// bakiye çizgisini (trend), farkın kaynağını ve gerçekleşen ödemeler listesini
/// sunan ayrıntı sayfası (EK-V12, S68-6, 7, GS32).
/// </summary>
public partial class HistoryDetailPage : ContentPage, IQueryAttributable
{
    private readonly HistoryDetailViewModel _viewModel;
    private Guid? _requestedActualId;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public HistoryDetailPage(HistoryDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen gerçekleşme kimliğini saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(Routes.HistoryActualIdParameter, out var raw))
        {
            if (raw is Guid guid)
            {
                _requestedActualId = guid;
            }
            else if (raw is string str && Guid.TryParse(str, out var parsed))
            {
                _requestedActualId = parsed;
            }
        }
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(_requestedActualId);
            _requestedActualId = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HistoryDetailPage yükleme hatası: {ex}");
        }
    }
}
