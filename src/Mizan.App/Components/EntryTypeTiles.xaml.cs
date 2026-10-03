using System.Windows.Input;
using Mizan.Presentation.Models;

namespace Mizan.App.Components;

/// <summary>
/// "Ne eklemek istiyorsun?" sorusunun bir grubunu karo olarak sunmak için vardır: renkli nokta ve grup
/// başlığı, altında her biri ikon + başlık + alt satır taşıyan karolar (GS29). Düz sistem diyaloğu türleri
/// ayrımsız alt alta diziyordu; grup rengi ve ikon türü okumadan tanıtır. Rengi sayfa verir
/// (<c>DynamicResource</c>, GK8): bileşen hangi grupta ve hangi temada olduğunu bilmez. İki ekran kullanır:
/// Finansal Yapı (V6f) ve simülatör (V10c).
/// </summary>
public partial class EntryTypeTiles : ContentView
{
    /// <summary>Gösterilen bölüm: grup ve karoları.</summary>
    public static readonly BindableProperty SectionProperty =
        BindableProperty.Create(nameof(Section), typeof(EntryTypeSection), typeof(EntryTypeTiles), null);

    /// <summary>Karoya dokununca çalışan komut; parametresi karonun öğesidir.</summary>
    public static readonly BindableProperty OptionCommandProperty =
        BindableProperty.Create(nameof(OptionCommand), typeof(ICommand), typeof(EntryTypeTiles), null);

    /// <summary>Grubun rengi: başlık noktası ve ikon.</summary>
    public static readonly BindableProperty AccentProperty =
        BindableProperty.Create(nameof(Accent), typeof(Color), typeof(EntryTypeTiles), null);

    /// <summary>Grubun yüzeyi: ikonun arkasındaki kare.</summary>
    public static readonly BindableProperty AccentSurfaceProperty =
        BindableProperty.Create(nameof(AccentSurface), typeof(Color), typeof(EntryTypeTiles), null);

    /// <summary>Bileşeni başlatır.</summary>
    public EntryTypeTiles()
    {
        InitializeComponent();
    }

    /// <summary>Gösterilen bölüm.</summary>
    public EntryTypeSection? Section
    {
        get => (EntryTypeSection?)GetValue(SectionProperty);
        set => SetValue(SectionProperty, value);
    }

    /// <summary>Karo komutu.</summary>
    public ICommand? OptionCommand
    {
        get => (ICommand?)GetValue(OptionCommandProperty);
        set => SetValue(OptionCommandProperty, value);
    }

    /// <summary>Grubun rengi.</summary>
    public Color? Accent
    {
        get => (Color?)GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    /// <summary>Grubun yüzeyi.</summary>
    public Color? AccentSurface
    {
        get => (Color?)GetValue(AccentSurfaceProperty);
        set => SetValue(AccentSurfaceProperty, value);
    }
}
