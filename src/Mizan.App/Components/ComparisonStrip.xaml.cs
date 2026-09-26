namespace Mizan.App.Components;

/// <summary>
/// Kullanıcının planlanan, gerçekleşen ve aralarındaki farkı üç kompakt kutu içinde
/// yan yana görmesini sağlayarak finansal sapmayı tek bakışta algılatmak için vardır.
/// </summary>
public partial class ComparisonStrip : ContentView
{
    /// <summary>Plan kutusu başlığını belirler (varsayılan "PLAN").</summary>
    public static readonly BindableProperty PlanTitleProperty =
        BindableProperty.Create(nameof(PlanTitle), typeof(string), typeof(ComparisonStrip), "PLAN");

    /// <summary>Planlanan tutar değerini belirler.</summary>
    public static readonly BindableProperty PlanValueProperty =
        BindableProperty.Create(nameof(PlanValue), typeof(string), typeof(ComparisonStrip), string.Empty);

    /// <summary>Gerçekleşen kutusu başlığını belirler (varsayılan "GERÇEK").</summary>
    public static readonly BindableProperty ActualTitleProperty =
        BindableProperty.Create(nameof(ActualTitle), typeof(string), typeof(ComparisonStrip), "GERÇEK");

    /// <summary>Gerçekleşen tutar değerini belirler.</summary>
    public static readonly BindableProperty ActualValueProperty =
        BindableProperty.Create(nameof(ActualValue), typeof(string), typeof(ComparisonStrip), string.Empty);

    /// <summary>Fark kutusu başlığını belirler (varsayılan "FARK").</summary>
    public static readonly BindableProperty DifferenceTitleProperty =
        BindableProperty.Create(nameof(DifferenceTitle), typeof(string), typeof(ComparisonStrip), "FARK");

    /// <summary>Fark tutar değerini belirler.</summary>
    public static readonly BindableProperty DifferenceValueProperty =
        BindableProperty.Create(nameof(DifferenceValue), typeof(string), typeof(ComparisonStrip), string.Empty);

    /// <summary>Fark kutusunun semantik rengini belirler (Positive, Negative, Warning, Default).</summary>
    public static readonly BindableProperty DifferenceSemanticProperty =
        BindableProperty.Create(nameof(DifferenceSemantic), typeof(string), typeof(ComparisonStrip), "Default", propertyChanged: OnDifferenceSemanticChanged);

    /// <summary>ComparisonStrip bileşenini başlatır.</summary>
    public ComparisonStrip()
    {
        InitializeComponent();
    }

    /// <summary>Plan başlığını alır veya ayarlar.</summary>
    public string PlanTitle
    {
        get => (string)GetValue(PlanTitleProperty);
        set => SetValue(PlanTitleProperty, value);
    }

    /// <summary>Plan değerini alır veya ayarlar.</summary>
    public string PlanValue
    {
        get => (string)GetValue(PlanValueProperty);
        set => SetValue(PlanValueProperty, value);
    }

    /// <summary>Gerçekleşen başlığını alır veya ayarlar.</summary>
    public string ActualTitle
    {
        get => (string)GetValue(ActualTitleProperty);
        set => SetValue(ActualTitleProperty, value);
    }

    /// <summary>Gerçekleşen değerini alır veya ayarlar.</summary>
    public string ActualValue
    {
        get => (string)GetValue(ActualValueProperty);
        set => SetValue(ActualValueProperty, value);
    }

    /// <summary>Fark başlığını alır veya ayarlar.</summary>
    public string DifferenceTitle
    {
        get => (string)GetValue(DifferenceTitleProperty);
        set => SetValue(DifferenceTitleProperty, value);
    }

    /// <summary>Fark değerini alır veya ayarlar.</summary>
    public string DifferenceValue
    {
        get => (string)GetValue(DifferenceValueProperty);
        set => SetValue(DifferenceValueProperty, value);
    }

    /// <summary>Fark semantik durumunu alır veya ayarlar.</summary>
    public string DifferenceSemantic
    {
        get => (string)GetValue(DifferenceSemanticProperty);
        set => SetValue(DifferenceSemanticProperty, value);
    }

    private static void OnDifferenceSemanticChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ComparisonStrip strip && strip.DifferenceBorder is not null && strip.DifferenceFigureLabel is not null)
        {
            var semantic = strip.DifferenceSemantic?.Trim();
            var (surfaceKey, textKey) = semantic switch
            {
                "Positive" => ("PositiveSurface", "PositiveText"),
                "Negative" => ("NegativeSurface", "NegativeText"),
                "Warning" => ("WarningSurface", "WarningText"),
                _ => ("ActualSurface", "TextPrimary")
            };

            strip.DifferenceBorder.SetDynamicResource(Border.BackgroundColorProperty, surfaceKey);
            strip.DifferenceFigureLabel.SetDynamicResource(Label.TextColorProperty, textKey);
        }
    }
}
