using Microsoft.Maui.Graphics;
using Mizan.Presentation.Charts;

namespace Mizan.App.Charts;

/// <summary>
/// Bir grafiğin tarih ve tutar değerlerini çizim alanındaki piksele çevirmek için vardır; böylece
/// <see cref="AreaTrend"/> her çizgi, dolgu ve işaret için aynı eşleme formülünü yeniden yazmaz.
/// Yatay eksen sıra numarası değil tarihtir: iki nokta arasındaki mesafe aradaki gün sayısıyla orantılıdır.
/// </summary>
internal readonly struct ChartScale
{
    private const float MinimumRange = 0.0001f;

    private readonly RectF _rect;
    private readonly DateOnly _start;
    private readonly DateOnly _end;
    private readonly float _min;
    private readonly float _range;

    /// <param name="rect">Çizim alanı.</param>
    /// <param name="start">Yatay eksenin ilk günü.</param>
    /// <param name="end">Yatay eksenin son günü.</param>
    /// <param name="min">Dikey eksenin en küçük tutarı.</param>
    /// <param name="max">Dikey eksenin en büyük tutarı.</param>
    public ChartScale(RectF rect, DateOnly start, DateOnly end, decimal min, decimal max)
    {
        _rect = rect;
        _start = start;
        _end = end;
        _min = (float)min;
        _range = Math.Max((float)max - (float)min, MinimumRange);
    }

    /// <summary>Verilen gün yatay eksenin içinde mi?</summary>
    public bool Contains(DateOnly date) => date >= _start && date <= _end;

    /// <summary>Günün yatay konumu; eksen tek güne inmişse sol kenar.</summary>
    public float X(DateOnly date)
    {
        var span = _end.DayNumber - _start.DayNumber;
        if (span <= 0) { return _rect.Left; }

        return _rect.Left + ((date.DayNumber - _start.DayNumber) / (float)span * _rect.Width);
    }

    /// <summary>Tutarın dikey konumu; en küçük tutar alt kenarda durur.</summary>
    public float Y(decimal value) => _rect.Bottom - (((float)value - _min) / _range * _rect.Height);

    /// <summary>Noktaları sırayla birleştiren çizgi yolu.</summary>
    public PathF LinePath(IReadOnlyList<ChartPoint> points)
    {
        var path = new PathF();
        for (var i = 0; i < points.Count; i++)
        {
            var (x, y) = (X(points[i].Date), Y(points[i].Value));
            if (i == 0) { path.MoveTo(x, y); } else { path.LineTo(x, y); }
        }
        return path;
    }

    /// <summary>
    /// Çizgiyle taban arasını dolduran kapalı yol. Taban verilmezse alt kenardır; verilirse o tutardır ve tabanın
    /// altına inen bölüm ayrı bir cep olarak dolar (12 dönemde sıfır: eksi dönemler, GS26-4).
    /// </summary>
    public PathF AreaPath(IReadOnlyList<ChartPoint> points, decimal? baseline = null)
    {
        var baseY = baseline is { } level ? Y(level) : _rect.Bottom;
        var path = new PathF();
        path.MoveTo(X(points[0].Date), baseY);
        foreach (var point in points)
        {
            path.LineTo(X(point.Date), Y(point.Value));
        }
        path.LineTo(X(points[^1].Date), baseY);
        path.Close();
        return path;
    }
}
