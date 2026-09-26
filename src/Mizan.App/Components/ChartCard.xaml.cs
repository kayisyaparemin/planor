namespace Mizan.App.Components;

/// <summary>
/// Sayfadaki tekil finansal grafik yüzeyini (GS10 ve GK4 kuralı gereğince ekran başına en fazla 1)
/// standart başlık ve lejant çipiyle çerçeveleyerek sunmak için vardır.
/// </summary>
public partial class ChartCard : ContentView
{
    /// <summary>Grafik kartı ana başlığını belirler.</summary>
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ChartCard), string.Empty);

    /// <summary>Başlığın yanındaki küçük lejant çipi metnini belirler.</summary>
    public static readonly BindableProperty LegendTextProperty =
        BindableProperty.Create(nameof(LegendText), typeof(string), typeof(ChartCard), string.Empty, propertyChanged: OnMetaChanged);

    /// <summary>Çizilecek IDrawable grafik primitifini belirler.</summary>
    public static readonly BindableProperty DrawableProperty =
        BindableProperty.Create(nameof(Drawable), typeof(IDrawable), typeof(ChartCard), null, propertyChanged: OnMetaChanged);

    /// <summary>Özel grafik görünüm içeriğini belirler.</summary>
    public static readonly BindableProperty ChartContentProperty =
        BindableProperty.Create(nameof(ChartContent), typeof(View), typeof(ChartCard), null, propertyChanged: OnMetaChanged);

    /// <summary>Grafik çizim alanı yüksekliğini belirler (varsayılan 160).</summary>
    public static readonly BindableProperty ChartHeightProperty =
        BindableProperty.Create(nameof(ChartHeight), typeof(double), typeof(ChartCard), 160.0);

    private static readonly BindablePropertyKey HasLegendPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasLegend), typeof(bool), typeof(ChartCard), false);

    /// <summary>Lejant çipinin dolu olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasLegendProperty = HasLegendPropertyKey.BindableProperty;

    private static readonly BindablePropertyKey HasDrawablePropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasDrawable), typeof(bool), typeof(ChartCard), false);

    /// <summary>IDrawable grafiğin mevcut olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasDrawableProperty = HasDrawablePropertyKey.BindableProperty;

    private static readonly BindablePropertyKey HasChartContentPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasChartContent), typeof(bool), typeof(ChartCard), false);

    /// <summary>Özel grafik içeriğinin mevcut olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasChartContentProperty = HasChartContentPropertyKey.BindableProperty;

    /// <summary>ChartCard bileşenini başlatır.</summary>
    public ChartCard()
    {
        InitializeComponent();
    }

    /// <summary>Kart başlığını alır veya ayarlar.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Lejant çipi metnini alır veya ayarlar.</summary>
    public string LegendText
    {
        get => (string)GetValue(LegendTextProperty);
        set => SetValue(LegendTextProperty, value);
    }

    /// <summary>Çizilecek grafiği alır veya ayarlar.</summary>
    public IDrawable? Drawable
    {
        get => (IDrawable?)GetValue(DrawableProperty);
        set => SetValue(DrawableProperty, value);
    }

    /// <summary>Özel grafik görünümünü alır veya ayarlar.</summary>
    public View? ChartContent
    {
        get => (View?)GetValue(ChartContentProperty);
        set => SetValue(ChartContentProperty, value);
    }

    /// <summary>Grafik yüksekliğini alır veya ayarlar.</summary>
    public double ChartHeight
    {
        get => (double)GetValue(ChartHeightProperty);
        set => SetValue(ChartHeightProperty, value);
    }

    /// <summary>Lejant çipinin görünür olup olmadığını alır.</summary>
    public bool HasLegend => (bool)GetValue(HasLegendProperty);

    /// <summary>IDrawable alanının görünür olup olmadığını alır.</summary>
    public bool HasDrawable => (bool)GetValue(HasDrawableProperty);

    /// <summary>Özel grafik içeriğinin görünür olup olmadığını alır.</summary>
    public bool HasChartContent => (bool)GetValue(HasChartContentProperty);

    private static void OnMetaChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ChartCard card)
        {
            card.SetValue(HasLegendPropertyKey, !string.IsNullOrWhiteSpace(card.LegendText));
            card.SetValue(HasDrawablePropertyKey, card.Drawable is not null);
            card.SetValue(HasChartContentPropertyKey, card.ChartContent is not null && card.Drawable is null);
        }
    }
}
