using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Taksitli ödeme planını ekleyen ya da düzenleyen form sayfası (EK-V6e). Finansal Yapı'dan
/// kimliksiz (yeni plan) ya da <see cref="Routes.PlanIdParameter"/> ile (düzenleme) açılır.
/// </summary>
public partial class PaymentPlanFormPage : ContentPage, IQueryAttributable
{
    private readonly PaymentPlanFormViewModel _viewModel;
    private Guid? _planId;
    private bool _loaded;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public PaymentPlanFormPage(PaymentPlanFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen plan kimliğini yüklemeye saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _planId = query.TryGetValue(Routes.PlanIdParameter, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
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
            await _viewModel.LoadCommand.ExecuteAsync(_planId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PaymentPlanFormPage yükleme hatası: {ex}");
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
