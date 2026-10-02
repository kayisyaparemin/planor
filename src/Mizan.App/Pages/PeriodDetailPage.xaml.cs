using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// 12 Dönem'deki bir dönemin sonunda ne kalacağını, bu rakamın nereden çıktığını, o dönemin ödemelerini ve kart
/// faizini gösteren sayfa (EK-V9). Karodan <see cref="Routes.PeriodStartParameter"/> ile açılır. Kart Kontrol'den
/// dönünce ödeme şekli değişmiş olabileceği için her görünüşte aynı dönemi yeniden yükler (S75-1).
/// </summary>
public partial class PeriodDetailPage : ContentPage, IQueryAttributable
{
    private readonly PeriodDetailViewModel _viewModel;
    private DateOnly? _requestedPeriodStart;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public PeriodDetailPage(PeriodDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen dönemi bir sonraki yüklemeye saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _requestedPeriodStart = query.TryGetValue(Routes.PeriodStartParameter, out var raw) && raw is DateOnly start
            ? start
            : null;
    }

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(_requestedPeriodStart);
            _requestedPeriodStart = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PeriodDetailPage yükleme hatası: {ex}");
        }
    }
}
