namespace Mizan.App.Components;

/// <summary>
/// Kullanıcının aktif nakit akış döneminin adını, geçen gün sayacını ve kalan süreyi
/// tek bir yatay ilerleme şeridinde özetleyerek dönem zaman bilincini canlı tutmak için vardır.
/// </summary>
public partial class PeriodRail : ContentView
{
    /// <summary>Dönem adını belirler (örn. "15 Eylül 2026 Dönemi").</summary>
    public static readonly BindableProperty PeriodNameProperty =
        BindableProperty.Create(nameof(PeriodName), typeof(string), typeof(PeriodRail), string.Empty);

    /// <summary>Dönem gün sayacı metnini belirler (örn. "4/30 gün · 15 Ekim").</summary>
    public static readonly BindableProperty DayCountTextProperty =
        BindableProperty.Create(nameof(DayCountText), typeof(string), typeof(PeriodRail), string.Empty);

    /// <summary>Dönem ilerleme oranını belirler (0.0 ile 1.0 arası).</summary>
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(PeriodRail), 0.0);

    /// <summary>PeriodRail bileşenini başlatır.</summary>
    public PeriodRail()
    {
        InitializeComponent();
    }

    /// <summary>Dönem adını alır veya ayarlar.</summary>
    public string PeriodName
    {
        get => (string)GetValue(PeriodNameProperty);
        set => SetValue(PeriodNameProperty, value);
    }

    /// <summary>Gün sayacı metnini alır veya ayarlar.</summary>
    public string DayCountText
    {
        get => (string)GetValue(DayCountTextProperty);
        set => SetValue(DayCountTextProperty, value);
    }

    /// <summary>İlerleme oranını (0.0 - 1.0) alır veya ayarlar.</summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }
}
