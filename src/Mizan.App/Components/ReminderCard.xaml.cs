using System.Windows.Input;

namespace Mizan.App.Components;

/// <summary>
/// Kullanıcıya vadesi gelen veya geciken bir ödeme hatırlatıcısını sunmak ve
/// "Ödedim" ya da "Ertele" gibi iki net aksiyonla hızlı yanıt aldırmak için vardır.
/// </summary>
public partial class ReminderCard : ContentView
{
    /// <summary>Hatırlatıcı kartı başlığını belirler.</summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ReminderCard), string.Empty);

    /// <summary>Hatırlatıcı detay mesajını veya vade açıklamasını belirler.</summary>
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(ReminderCard), string.Empty);

    /// <summary>Birincil aksiyon buton metnini belirler (varsayılan "Ödedim").</summary>
    public static readonly BindableProperty PrimaryActionTextProperty =
        BindableProperty.Create(nameof(PrimaryActionText), typeof(string), typeof(ReminderCard), "Ödedim");

    /// <summary>Birincil aksiyon komutunu belirler.</summary>
    public static readonly BindableProperty PrimaryActionCommandProperty =
        BindableProperty.Create(nameof(PrimaryActionCommand), typeof(ICommand), typeof(ReminderCard), null);

    /// <summary>Birincil aksiyon komut parametresini belirler.</summary>
    public static readonly BindableProperty PrimaryActionCommandParameterProperty =
        BindableProperty.Create(nameof(PrimaryActionCommandParameter), typeof(object), typeof(ReminderCard), null);

    /// <summary>İkincil aksiyon buton metnini belirler (varsayılan "Ertele").</summary>
    public static readonly BindableProperty SecondaryActionTextProperty =
        BindableProperty.Create(nameof(SecondaryActionText), typeof(string), typeof(ReminderCard), "Ertele");

    /// <summary>İkincil aksiyon komutunu belirler.</summary>
    public static readonly BindableProperty SecondaryActionCommandProperty =
        BindableProperty.Create(nameof(SecondaryActionCommand), typeof(ICommand), typeof(ReminderCard), null);

    /// <summary>İkincil aksiyon komut parametresini belirler.</summary>
    public static readonly BindableProperty SecondaryActionCommandParameterProperty =
        BindableProperty.Create(nameof(SecondaryActionCommandParameter), typeof(object), typeof(ReminderCard), null);

    /// <summary>ReminderCard bileşenini başlatır.</summary>
    public ReminderCard()
    {
        InitializeComponent();
    }

    /// <summary>Kart başlığını alır veya ayarlar.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Detay mesajını alır veya ayarlar.</summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Birincil aksiyon metnini alır veya ayarlar.</summary>
    public string PrimaryActionText
    {
        get => (string)GetValue(PrimaryActionTextProperty);
        set => SetValue(PrimaryActionTextProperty, value);
    }

    /// <summary>Birincil aksiyon komutunu alır veya ayarlar.</summary>
    public ICommand? PrimaryActionCommand
    {
        get => (ICommand?)GetValue(PrimaryActionCommandProperty);
        set => SetValue(PrimaryActionCommandProperty, value);
    }

    /// <summary>Birincil aksiyon parametresini alır veya ayarlar.</summary>
    public object? PrimaryActionCommandParameter
    {
        get => GetValue(PrimaryActionCommandParameterProperty);
        set => SetValue(PrimaryActionCommandParameterProperty, value);
    }

    /// <summary>İkincil aksiyon metnini alır veya ayarlar.</summary>
    public string SecondaryActionText
    {
        get => (string)GetValue(SecondaryActionTextProperty);
        set => SetValue(SecondaryActionTextProperty, value);
    }

    /// <summary>İkincil aksiyon komutunu alır veya ayarlar.</summary>
    public ICommand? SecondaryActionCommand
    {
        get => (ICommand?)GetValue(SecondaryActionCommandProperty);
        set => SetValue(SecondaryActionCommandProperty, value);
    }

    /// <summary>İkincil aksiyon parametresini alır veya ayarlar.</summary>
    public object? SecondaryActionCommandParameter
    {
        get => GetValue(SecondaryActionCommandParameterProperty);
        set => SetValue(SecondaryActionCommandParameterProperty, value);
    }
}
