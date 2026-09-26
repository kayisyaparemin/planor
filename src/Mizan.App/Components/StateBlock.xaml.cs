using System.Windows.Input;

namespace Mizan.App.Components;

/// <summary>
/// Sayfalarda boş veri veya hata durumlarını standart bir dille karşılamak,
/// durumu açıklamak ve aksiyon aldırmak için vardır.
/// </summary>
public partial class StateBlock : ContentView
{
    /// <summary>Mevcut durumu belirler (Empty, Error).</summary>
    public static readonly BindableProperty CurrentStateProperty =
        BindableProperty.Create(
            nameof(CurrentState),
            typeof(string),
            typeof(StateBlock),
            "Empty",
            propertyChanged: OnStateChanged);

    /// <summary>Durum ikon glifini belirler.</summary>
    public static readonly BindableProperty IconGlyphProperty =
        BindableProperty.Create(
            nameof(IconGlyph),
            typeof(string),
            typeof(StateBlock),
            string.Empty,
            propertyChanged: OnMetaChanged);

    /// <summary>Durum açıklama mesajını belirler.</summary>
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(
            nameof(Message),
            typeof(string),
            typeof(StateBlock),
            string.Empty);

    /// <summary>Aksiyon buton metnini belirler.</summary>
    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(
            nameof(ActionText),
            typeof(string),
            typeof(StateBlock),
            string.Empty,
            propertyChanged: OnMetaChanged);

    /// <summary>Aksiyon komutunu belirler.</summary>
    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(
            nameof(ActionCommand),
            typeof(ICommand),
            typeof(StateBlock),
            null);

    /// <summary>Aksiyon komut parametresini belirler.</summary>
    public static readonly BindableProperty ActionCommandParameterProperty =
        BindableProperty.Create(
            nameof(ActionCommandParameter),
            typeof(object),
            typeof(StateBlock),
            null);

    private static readonly BindablePropertyKey HasIconPropertyKey =
        BindableProperty.CreateReadOnly(
            nameof(HasIcon),
            typeof(bool),
            typeof(StateBlock),
            false);

    /// <summary>İkonun dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasIconProperty = HasIconPropertyKey.BindableProperty;

    private static readonly BindablePropertyKey HasActionPropertyKey =
        BindableProperty.CreateReadOnly(
            nameof(HasAction),
            typeof(bool),
            typeof(StateBlock),
            false);

    /// <summary>Aksiyon butonunun dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasActionProperty = HasActionPropertyKey.BindableProperty;

    /// <summary>StateBlock bileşenini başlatır.</summary>
    public StateBlock()
    {
        InitializeComponent();
    }

    /// <summary>Mevcut durumu alır veya ayarlar.</summary>
    public string CurrentState
    {
        get => (string)GetValue(CurrentStateProperty);
        set => SetValue(CurrentStateProperty, value);
    }

    /// <summary>İkon glifini alır veya ayarlar.</summary>
    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>Açıklama mesajını alır veya ayarlar.</summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
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

    /// <summary>İkonun görünür olup olmadığını alır.</summary>
    public bool HasIcon => (bool)GetValue(HasIconProperty);

    /// <summary>Aksiyon butonunun görünür olup olmadığını alır.</summary>
    public bool HasAction => (bool)GetValue(HasActionProperty);

    private static void OnMetaChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StateBlock block)
        {
            block.SetValue(HasIconPropertyKey, !string.IsNullOrWhiteSpace(block.IconGlyph));
            block.SetValue(HasActionPropertyKey, !string.IsNullOrWhiteSpace(block.ActionText));
        }
    }

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StateBlock block)
        {
            var state = block.CurrentState?.Trim();
            bool isError = string.Equals(state, "Error", StringComparison.OrdinalIgnoreCase);

            var iconColorKey = isError ? "NegativeText" : "TextSecondary";
            var messageColorKey = isError ? "TextPrimary" : "TextSecondary";

            block.StateIconLabel?.SetDynamicResource(Label.TextColorProperty, iconColorKey);
            block.StateMessageLabel?.SetDynamicResource(Label.TextColorProperty, messageColorKey);
        }
    }
}
