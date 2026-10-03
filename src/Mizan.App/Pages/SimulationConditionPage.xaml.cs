using Mizan.Presentation.Navigation;
using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Simülatörün deneme formu (EK-V10). "Ekle" seçicisinden senaryo seçeneğinin anahtarıyla
/// (<see cref="Routes.ScenarioOptionParameter"/>, yeni deneme) ya da satırdan deneme kimliğiyle
/// (<see cref="Routes.ConditionIdParameter"/>, düzenleme) açılır.
/// </summary>
public partial class SimulationConditionPage : ContentPage, IQueryAttributable
{
    private readonly SimulationConditionViewModel _viewModel;
    private bool _loaded;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public SimulationConditionPage(SimulationConditionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Gezinmeyle gelen seçenek anahtarını ya da deneme kimliğini forma verir.</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var optionKey = query.TryGetValue(Routes.ScenarioOptionParameter, out var option) ? option?.ToString() : null;
        Guid? conditionId = query.TryGetValue(Routes.ConditionIdParameter, out var raw) && Guid.TryParse(raw?.ToString(), out var id)
            ? id
            : null;
        Guid? cardId = query.TryGetValue(Routes.CardIdParameter, out var rawCard) && Guid.TryParse(rawCard?.ToString(), out var cid)
            ? cid
            : null;
        Guid? loanId = query.TryGetValue(Routes.LoanIdParameter, out var rawLoan) && Guid.TryParse(rawLoan?.ToString(), out var lid)
            ? lid
            : null;
        _viewModel.Prepare(optionKey, conditionId, cardId, loanId);
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
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SimulationConditionPage yükleme hatası: {ex}");
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
