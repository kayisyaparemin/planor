using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Finansal Yapı "Ekle"sinin açtığı kayıt türü seçici sayfası (EK-V6f). Seçilen tür bu sayfanın yerine
/// kendi formunu açar; formdan dönüş doğrudan listeye iner (S77-2). "Hangi kart?" seviyesindeyken geri,
/// listeye değil karolara döner (S77-5).
/// </summary>
public partial class RecordEntryPickerPage : ContentPage
{
    private readonly RecordEntryPickerViewModel _viewModel;

    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public RecordEntryPickerPage(RecordEntryPickerViewModel viewModel)
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
