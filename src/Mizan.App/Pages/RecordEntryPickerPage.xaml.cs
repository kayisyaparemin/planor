using Mizan.Presentation.ViewModels;

namespace Mizan.App.Pages;

/// <summary>
/// Finansal Yapı "Ekle"sinin açtığı kayıt türü seçici sayfası (EK-V6f). Seçilen tür bu sayfanın yerine
/// kendi formunu açar; formdan dönüş doğrudan listeye iner (S77-2).
/// </summary>
public partial class RecordEntryPickerPage : ContentPage
{
    /// <summary>Sayfayı görünüm modeliyle başlatır.</summary>
    public RecordEntryPickerPage(RecordEntryPickerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}
