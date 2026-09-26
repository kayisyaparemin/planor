using System.Windows.Input;

namespace Mizan.App.Components;

/// <summary>
/// Sayfalar veya bölümler arası gezinmeyi sağlayan, solunda opsiyonel ikon,
/// ortasında açıklayıcı metin ve sağında standart gezinme oku taşıyan satır bileşenidir.
/// </summary>
public partial class NavRow : ContentView
{
    /// <summary>Sol baştaki ikon glifini belirler.</summary>
    public static readonly BindableProperty IconGlyphProperty =
        BindableProperty.Create(nameof(IconGlyph), typeof(string), typeof(NavRow), string.Empty, propertyChanged: OnMetaChanged);

    /// <summary>Satır başlık metnini belirler.</summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(NavRow), string.Empty);

    /// <summary>Başlığın altındaki opsiyonel açıklayıcı alt metni belirler.</summary>
    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(NavRow), string.Empty, propertyChanged: OnMetaChanged);

    /// <summary>Satıra tıklandığında çalışacak komutu belirler.</summary>
    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(NavRow), null);

    /// <summary>Gezinme komutuna iletilecek parametreyi belirler.</summary>
    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(NavRow), null);

    private static readonly BindablePropertyKey HasIconPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasIcon), typeof(bool), typeof(NavRow), false);

    /// <summary>İkonun dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasIconProperty = HasIconPropertyKey.BindableProperty;

    private static readonly BindablePropertyKey HasSubtitlePropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasSubtitle), typeof(bool), typeof(NavRow), false);

    /// <summary>Alt başlığın dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasSubtitleProperty = HasSubtitlePropertyKey.BindableProperty;

    /// <summary>NavRow bileşenini başlatır.</summary>
    public NavRow()
    {
        InitializeComponent();
    }

    /// <summary>İkon glifini alır veya ayarlar.</summary>
    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>Satır başlığını alır veya ayarlar.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Alt başlığı alır veya ayarlar.</summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Tıklama komutunu alır veya ayarlar.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Komut parametresini alır veya ayarlar.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>İkon alanının görünür olup olmadığını alır.</summary>
    public bool HasIcon => (bool)GetValue(HasIconProperty);

    /// <summary>Alt başlığın görünür olup olmadığını alır.</summary>
    public bool HasSubtitle => (bool)GetValue(HasSubtitleProperty);

    private static void OnMetaChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavRow row)
        {
            row.SetValue(HasIconPropertyKey, !string.IsNullOrWhiteSpace(row.IconGlyph));
            row.SetValue(HasSubtitlePropertyKey, !string.IsNullOrWhiteSpace(row.Subtitle));
        }
    }
}
