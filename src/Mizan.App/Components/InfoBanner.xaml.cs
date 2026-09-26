namespace Mizan.App.Components;

/// <summary>
/// Sayfada dönem durumu veya en kritik finansal içgörüyü tek bir dikkat çekici cümleyle
/// özetlemek (GS4 kuralı gereğince aynı anda en fazla bir uyarı) için vardır.
/// </summary>
public partial class InfoBanner : ContentView
{
    /// <summary>Açıklama veya bildirim mesajını belirler (en fazla 90 karakter).</summary>
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(InfoBanner), string.Empty);

    /// <summary>Mesajın solundaki opsiyonel ikon glifini belirler.</summary>
    public static readonly BindableProperty IconGlyphProperty =
        BindableProperty.Create(nameof(IconGlyph), typeof(string), typeof(InfoBanner), string.Empty, propertyChanged: OnIconChanged);

    private static readonly BindablePropertyKey HasIconPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasIcon), typeof(bool), typeof(InfoBanner), false);

    /// <summary>İkonun dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasIconProperty = HasIconPropertyKey.BindableProperty;

    /// <summary>InfoBanner bileşenini başlatır.</summary>
    public InfoBanner()
    {
        InitializeComponent();
    }

    /// <summary>Mesaj metnini alır veya ayarlar.</summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>İkon glifini alır veya ayarlar.</summary>
    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>İkonun görünür olup olmadığını alır.</summary>
    public bool HasIcon => (bool)GetValue(HasIconProperty);

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is InfoBanner banner)
        {
            banner.SetValue(HasIconPropertyKey, !string.IsNullOrWhiteSpace(banner.IconGlyph));
        }
    }
}
