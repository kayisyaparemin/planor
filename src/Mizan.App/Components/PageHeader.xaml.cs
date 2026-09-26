using System.Windows.Input;

namespace Mizan.App.Components;

/// <summary>
/// Ekranların en üstünde yer alan standart başlık hiyerarşisini (kategori sloganı,
/// sayfa başlığı ve tekil opsiyonel aksiyon) sunmak ve ekranlar arası başlık tutarlılığını
/// sağlamak için vardır.
/// </summary>
public partial class PageHeader : ContentView
{
    /// <summary>Sayfa başlığının üstündeki küçük kategori etiketini belirler.</summary>
    public static readonly BindableProperty EyebrowTextProperty =
        BindableProperty.Create(nameof(EyebrowText), typeof(string), typeof(PageHeader), string.Empty, propertyChanged: OnHeaderChanged);

    /// <summary>Sayfanın ana başlık metnini belirler.</summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageHeader), string.Empty);

    /// <summary>Aksiyon alanında gösterilecek opsiyonel ikon karakterini belirler.</summary>
    public static readonly BindableProperty ActionIconProperty =
        BindableProperty.Create(nameof(ActionIcon), typeof(string), typeof(PageHeader), string.Empty, propertyChanged: OnHeaderChanged);

    /// <summary>Aksiyon alanında gösterilecek opsiyonel metin butonunu belirler.</summary>
    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(PageHeader), string.Empty, propertyChanged: OnHeaderChanged);

    /// <summary>Aksiyona tıklandığında çalıştırılacak komutu belirler.</summary>
    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(PageHeader), null);

    /// <summary>Aksiyon komutuna iletilecek parametreyi belirler.</summary>
    public static readonly BindableProperty ActionCommandParameterProperty =
        BindableProperty.Create(nameof(ActionCommandParameter), typeof(object), typeof(PageHeader), null);

    private static readonly BindablePropertyKey HasEyebrowPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasEyebrow), typeof(bool), typeof(PageHeader), false);

    /// <summary>Eyebrow metninin dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasEyebrowProperty = HasEyebrowPropertyKey.BindableProperty;

    private static readonly BindablePropertyKey HasActionIconPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasActionIcon), typeof(bool), typeof(PageHeader), false);

    /// <summary>Aksiyon ikonunun dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasActionIconProperty = HasActionIconPropertyKey.BindableProperty;

    private static readonly BindablePropertyKey HasActionTextPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasActionText), typeof(bool), typeof(PageHeader), false);

    /// <summary>Aksiyon metninin dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasActionTextProperty = HasActionTextPropertyKey.BindableProperty;

    /// <summary>PageHeader bileşenini başlatır.</summary>
    public PageHeader()
    {
        InitializeComponent();
    }

    /// <summary>Eyebrow metnini alır veya ayarlar.</summary>
    public string EyebrowText
    {
        get => (string)GetValue(EyebrowTextProperty);
        set => SetValue(EyebrowTextProperty, value);
    }

    /// <summary>Ana başlık metnini alır veya ayarlar.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Aksiyon ikonunu alır veya ayarlar.</summary>
    public string ActionIcon
    {
        get => (string)GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }

    /// <summary>Aksiyon metnini alır veya ayarlar.</summary>
    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    /// <summary>Aksiyon komutunu alır veya ayarlar.</summary>
    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    /// <summary>Aksiyon komut parametresini alır veya ayarlar.</summary>
    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }

    /// <summary>Eyebrow alanının görünür olup olmadığını alır.</summary>
    public bool HasEyebrow => (bool)GetValue(HasEyebrowProperty);

    /// <summary>Aksiyon ikonunun görünür olup olmadığını alır.</summary>
    public bool HasActionIcon => (bool)GetValue(HasActionIconProperty);

    /// <summary>Aksiyon metninin görünür olup olmadığını alır.</summary>
    public bool HasActionText => (bool)GetValue(HasActionTextProperty);

    private static void OnHeaderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PageHeader header)
        {
            header.SetValue(HasEyebrowPropertyKey, !string.IsNullOrWhiteSpace(header.EyebrowText));
            header.SetValue(HasActionIconPropertyKey, !string.IsNullOrWhiteSpace(header.ActionIcon) && string.IsNullOrWhiteSpace(header.ActionText));
            header.SetValue(HasActionTextPropertyKey, !string.IsNullOrWhiteSpace(header.ActionText));
        }
    }
}
