using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Planlı büyük harcamayı ekleyen ya da düzenleyen form sayfası (EK-V6e). Finansal Yapı'dan
/// kimliksiz (yeni harcama) ya da <see cref="Routes.ExpenseIdParameter"/> ile (düzenleme) açılır.
/// </summary>
public partial class PlannedExpenseFormPage : ContentPage, IQueryAttributable
{
    private readonly PlannedExpenseFormViewModel _viewModel;
    private Guid? _expenseId;
    private bool _loaded;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public PlannedExpenseFormPage(PlannedExpenseFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen harcama kimliğini yüklemeye saklar.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _expenseId = query.TryGetValue(Routes.ExpenseIdParameter, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
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
            await _viewModel.LoadCommand.ExecuteAsync(_expenseId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PlannedExpenseFormPage yükleme hatası: {ex}");
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
