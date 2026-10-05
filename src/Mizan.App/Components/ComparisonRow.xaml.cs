namespace Mizan.App.Components;

/// <summary>
/// Bir kalemin planlanan ve şu anki tutarını aynı satırda, alt alta hizalı sütunlarda gösterir (GS34-2).
/// <c>MetricRow</c> tek değer taşır; ikinci bir değer onun <c>Auto</c> kolonlarına eklenince plan rakamı her satırda
/// başka yere kayıyordu. Plandan sapma, iki rakamın aynı hizada durduğu tabloda tek bakışta okunur (S87).
/// </summary>
public partial class ComparisonRow : ContentView
{
    /// <summary>Satırın solundaki kalem adını belirler.</summary>
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(ComparisonRow), string.Empty);

    /// <summary>Planlanan tutarın metnini belirler.</summary>
    public static readonly BindableProperty PlanTextProperty =
        BindableProperty.Create(nameof(PlanText), typeof(string), typeof(ComparisonRow), string.Empty);

    /// <summary>Şu anki tutarın metnini belirler.</summary>
    public static readonly BindableProperty CurrentTextProperty =
        BindableProperty.Create(nameof(CurrentText), typeof(string), typeof(ComparisonRow), string.Empty);

    /// <summary>Şu anki tutarın semantik rengini belirler (Default, Negative).</summary>
    public static readonly BindableProperty SemanticProperty =
        BindableProperty.Create(nameof(Semantic), typeof(string), typeof(ComparisonRow), "Default", propertyChanged: OnSemanticChanged);

    /// <summary>ComparisonRow bileşenini başlatır.</summary>
    public ComparisonRow()
    {
        InitializeComponent();
    }

    /// <summary>Kalem adını alır veya ayarlar.</summary>
    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>Planlanan tutar metnini alır veya ayarlar.</summary>
    public string PlanText
    {
        get => (string)GetValue(PlanTextProperty);
        set => SetValue(PlanTextProperty, value);
    }

    /// <summary>Şu anki tutar metnini alır veya ayarlar.</summary>
    public string CurrentText
    {
        get => (string)GetValue(CurrentTextProperty);
        set => SetValue(CurrentTextProperty, value);
    }

    /// <summary>Şu anki tutarın semantik renk türünü alır veya ayarlar.</summary>
    public string Semantic
    {
        get => (string)GetValue(SemanticProperty);
        set => SetValue(SemanticProperty, value);
    }

    // Yalnız planı aşan şu an renklenir; planın altında kalmak olumlu sayılmaz (S87-3).
    private static void OnSemanticChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComparisonRow row && row.CurrentLabel is not null)
        {
            var textKey = row.Semantic?.Trim() == "Negative" ? "NegativeText" : "TextPrimary";
            row.CurrentLabel.SetDynamicResource(Label.TextColorProperty, textKey);
        }
    }
}
