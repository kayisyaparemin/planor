namespace Mizan.App.Components;

/// <summary>
/// Finansal listelerde veya kart içlerinde tek satırlık bir bilgi ve değer çiftini
/// (etiket ve tutar) semantik renk desteğiyle sunmak için vardır.
/// </summary>
public partial class MetricRow : ContentView
{
    /// <summary>Satırın solundaki açıklayıcı etiketi belirler.</summary>
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(MetricRow), string.Empty);

    /// <summary>Satırın sağındaki tutar veya değer metnini belirler.</summary>
    public static readonly BindableProperty ValueTextProperty =
        BindableProperty.Create(nameof(ValueText), typeof(string), typeof(MetricRow), string.Empty);

    /// <summary>Satırın sol başındaki opsiyonel ikon glifini belirler.</summary>
    public static readonly BindableProperty IconGlyphProperty =
        BindableProperty.Create(nameof(IconGlyph), typeof(string), typeof(MetricRow), string.Empty, propertyChanged: OnIconChanged);

    /// <summary>Değerin semantik rengini belirler (Default, Positive, Negative, Warning, Secondary).</summary>
    public static readonly BindableProperty SemanticProperty =
        BindableProperty.Create(nameof(Semantic), typeof(string), typeof(MetricRow), "Default", propertyChanged: OnSemanticChanged);

    private static readonly BindablePropertyKey HasIconPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasIcon), typeof(bool), typeof(MetricRow), false);

    /// <summary>İkonun dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasIconProperty = HasIconPropertyKey.BindableProperty;

    /// <summary>MetricRow bileşenini başlatır.</summary>
    public MetricRow()
    {
        InitializeComponent();
    }

    /// <summary>Etiket metnini alır veya ayarlar.</summary>
    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>Değer metnini alır veya ayarlar.</summary>
    public string ValueText
    {
        get => (string)GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    /// <summary>İkon glifini alır veya ayarlar.</summary>
    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>Semantik renk türünü alır veya ayarlar.</summary>
    public string Semantic
    {
        get => (string)GetValue(SemanticProperty);
        set => SetValue(SemanticProperty, value);
    }

    /// <summary>İkonun görünür olup olmadığını alır.</summary>
    public bool HasIcon => (bool)GetValue(HasIconProperty);

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MetricRow row)
        {
            row.SetValue(HasIconPropertyKey, !string.IsNullOrWhiteSpace(row.IconGlyph));
        }
    }

    private static void OnSemanticChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MetricRow row && row.ValueLabel is not null)
        {
            var semantic = row.Semantic?.Trim();
            var textKey = semantic switch
            {
                "Positive" => "PositiveText",
                "Negative" => "NegativeText",
                "Warning" => "WarningText",
                "Secondary" => "TextSecondary",
                _ => "TextPrimary"
            };

            row.ValueLabel.SetDynamicResource(Label.TextColorProperty, textKey);
        }
    }
}
