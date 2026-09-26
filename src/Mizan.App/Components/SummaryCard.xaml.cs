namespace Mizan.App.Components;

/// <summary>
/// Sayfada dönem sonu projeksiyonu veya toplam yükümlülük gibi anahtar bir finansal büyüklüğü
/// tek bir sağa yaslı büyük rakam ve açıklayıcı etiketle sunmak için vardır.
/// </summary>
public partial class SummaryCard : ContentView
{
    /// <summary>Üstteki küçük kategori etiketini belirler.</summary>
    public static readonly BindableProperty EyebrowTextProperty =
        BindableProperty.Create(nameof(EyebrowText), typeof(string), typeof(SummaryCard), string.Empty);

    /// <summary>Rakamın altındaki veya yanındaki tek açıklayıcı notu belirler.</summary>
    public static readonly BindableProperty NoteTextProperty =
        BindableProperty.Create(nameof(NoteText), typeof(string), typeof(SummaryCard), string.Empty, propertyChanged: OnNoteChanged);

    /// <summary>Sağa yaslı büyük rakam veya tutar metnini belirler.</summary>
    public static readonly BindableProperty FigureTextProperty =
        BindableProperty.Create(nameof(FigureText), typeof(string), typeof(SummaryCard), string.Empty);

    /// <summary>Rakamın semantik rengini belirler (Default, Positive, Negative, Warning).</summary>
    public static readonly BindableProperty FigureSemanticProperty =
        BindableProperty.Create(nameof(FigureSemantic), typeof(string), typeof(SummaryCard), "Default", propertyChanged: OnSemanticChanged);

    private static readonly BindablePropertyKey HasNotePropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasNote), typeof(bool), typeof(SummaryCard), false);

    /// <summary>Not metninin dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasNoteProperty = HasNotePropertyKey.BindableProperty;

    /// <summary>SummaryCard bileşenini başlatır.</summary>
    public SummaryCard()
    {
        InitializeComponent();
    }

    /// <summary>Eyebrow metnini alır veya ayarlar.</summary>
    public string EyebrowText
    {
        get => (string)GetValue(EyebrowTextProperty);
        set => SetValue(EyebrowTextProperty, value);
    }

    /// <summary>Not metnini alır veya ayarlar.</summary>
    public string NoteText
    {
        get => (string)GetValue(NoteTextProperty);
        set => SetValue(NoteTextProperty, value);
    }

    /// <summary>Büyük rakam metnini alır veya ayarlar.</summary>
    public string FigureText
    {
        get => (string)GetValue(FigureTextProperty);
        set => SetValue(FigureTextProperty, value);
    }

    /// <summary>Rakamın semantik rengini alır veya ayarlar.</summary>
    public string FigureSemantic
    {
        get => (string)GetValue(FigureSemanticProperty);
        set => SetValue(FigureSemanticProperty, value);
    }

    /// <summary>Notun görünür olup olmadığını alır.</summary>
    public bool HasNote => (bool)GetValue(HasNoteProperty);

    private static void OnNoteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SummaryCard card)
        {
            card.SetValue(HasNotePropertyKey, !string.IsNullOrWhiteSpace(card.NoteText));
        }
    }

    private static void OnSemanticChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SummaryCard card && card.FigureLabel is not null)
        {
            var semantic = card.FigureSemantic?.Trim();
            var resourceKey = semantic switch
            {
                "Positive" => "PositiveText",
                "Negative" => "NegativeText",
                "Warning" => "WarningText",
                _ => "TextPrimary"
            };

            card.FigureLabel.SetDynamicResource(Label.TextColorProperty, resourceKey);
        }
    }
}
