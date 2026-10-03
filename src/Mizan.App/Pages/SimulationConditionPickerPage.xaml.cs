using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Simülatörün "Ekle" seçici sayfası (EK-V10, S77 V10 notları i). Seçilen tür bu sayfanın yerine
/// deneme formunu açar. "Hangi kart?" seviyesindeyken geri, simülatöre değil karolara döner.
/// </summary>
public partial class SimulationConditionPickerPage : ContentPage
{
    private readonly SimulationConditionPickerViewModel _viewModel;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public SimulationConditionPickerPage(SimulationConditionPickerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    /// <summary>Cihazın geri tuşu da geri okla aynı yoldan geçer: ikinci seviyede karolara döner.</summary>
    protected override bool OnBackButtonPressed()
    {
        if (_viewModel.BackCommand.CanExecute(null))
        {
            _viewModel.BackCommand.Execute(null);
        }

        return true;
    }
}
