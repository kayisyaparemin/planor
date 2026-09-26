using System.Windows.Input;

namespace Mizan.App.Components;

/// <summary>
/// Sayfada kullanıcının en kritik finansal girdisini (mevcut anlık bakiye veya başlangıç tutarı)
/// tek bir odaklanmış hero kartı yüzeyinde toplamak ve doğrudan aksiyon aldırmak için vardır.
/// </summary>
public partial class HeroInputCard : ContentView
{
    /// <summary>Giriş alanının üstündeki etiketi belirler.</summary>
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(HeroInputCard), string.Empty);

    /// <summary>Giriş alanındaki tutar metnini çift yönlü belirler.</summary>
    public static readonly BindableProperty AmountTextProperty =
        BindableProperty.Create(nameof(AmountText), typeof(string), typeof(HeroInputCard), string.Empty, BindingMode.TwoWay);

    /// <summary>Giriş alanı yer tutucu metnini belirler.</summary>
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(HeroInputCard), string.Empty);

    /// <summary>Giriş alanı klavye türünü belirler.</summary>
    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(HeroInputCard), Keyboard.Numeric);

    /// <summary>Birincil aksiyon buton metnini belirler.</summary>
    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(HeroInputCard), string.Empty);

    /// <summary>Aksiyona tıklandığında çalışacak komutu belirler.</summary>
    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(HeroInputCard), null);

    /// <summary>Aksiyon komutuna iletilecek parametreyi belirler.</summary>
    public static readonly BindableProperty ActionCommandParameterProperty =
        BindableProperty.Create(nameof(ActionCommandParameter), typeof(object), typeof(HeroInputCard), null);

    /// <summary>Kartın altındaki tek cümlelik ipucu/açıklama metnini belirler.</summary>
    public static readonly BindableProperty HintTextProperty =
        BindableProperty.Create(nameof(HintText), typeof(string), typeof(HeroInputCard), string.Empty, propertyChanged: OnHintChanged);

    private static readonly BindablePropertyKey HasHintPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasHint), typeof(bool), typeof(HeroInputCard), false);

    /// <summary>İpucu metninin dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasHintProperty = HasHintPropertyKey.BindableProperty;

    /// <summary>HeroInputCard bileşenini başlatır.</summary>
    public HeroInputCard()
    {
        InitializeComponent();
    }

    /// <summary>Etiket metnini alır veya ayarlar.</summary>
    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>Tutar metnini alır veya ayarlar.</summary>
    public string AmountText
    {
        get => (string)GetValue(AmountTextProperty);
        set => SetValue(AmountTextProperty, value);
    }

    /// <summary>Yer tutucu metnini alır veya ayarlar.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Klavye türünü alır veya ayarlar.</summary>
    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    /// <summary>Aksiyon buton metnini alır veya ayarlar.</summary>
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

    /// <summary>İpucu metnini alır veya ayarlar.</summary>
    public string HintText
    {
        get => (string)GetValue(HintTextProperty);
        set => SetValue(HintTextProperty, value);
    }

    /// <summary>İpucu metninin dolu olup olmadığını alır.</summary>
    public bool HasHint => (bool)GetValue(HasHintProperty);

    private static void OnHintChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeroInputCard card)
        {
            card.SetValue(HasHintPropertyKey, !string.IsNullOrWhiteSpace(card.HintText));
        }
    }
}
