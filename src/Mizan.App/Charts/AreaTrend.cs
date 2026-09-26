using Microsoft.Maui.Graphics;
using Mizan.Presentation.Charts;

namespace Mizan.App.Charts;

/// <summary>
/// "Seviye zamanla eşiğin altına iniyor mu?" sorusunu cevaplar.
/// Dönem sonu bakiyesi 12 dönem boyunca sıfırın altına düşüyorsa kullanıcının
/// bunu tek bakışta görmesi gerekiyor; sayı listesi bu yönü göstermiyordu.
/// </summary>
public sealed class AreaTrend : IDrawable
{
    /// <summary>
    /// Gerçekleşen veya ana projeksiyon zaman serisi.
    /// </summary>
    public ChartSeries? Series { get; set; }

    /// <summary>
    /// Karşılaştırma amaçlı planlanan zaman serisi (kesikli çizgi ile gösterilir).
    /// </summary>
    public ChartSeries? PlannedSeries { get; set; }

    /// <summary>
    /// Kritik referans seviyesi veya KMH sınırı eşik değeri (kesikli yatay çizgi).
    /// </summary>
    public ChartThreshold? Threshold { get; set; }

    /// <summary>
    /// Çizim yüzeyine alan dolgulu trend grafiğini çizer.
    /// </summary>
    /// <param name="canvas">Çizim tuvali.</param>
    /// <param name="dirtyRect">Çizim alanı sınırları.</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (canvas == null || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) { return; }
        if (Series == null || Series.Points.Count == 0) { return; }

        var (min, max) = CalculateBounds();
        var range = Math.Max(max - min, 0.0001f);

        DrawAreaFill(canvas, dirtyRect, min, range);
        DrawThresholdLine(canvas, dirtyRect, min, range);
        DrawPlannedLine(canvas, dirtyRect, min, range);
        DrawActualLine(canvas, dirtyRect, min, range);
    }

    private (float Min, float Max) CalculateBounds()
    {
        var allValues = Series!.Points.Select(p => (float)p.Value).ToList();
        if (PlannedSeries != null)
        {
            allValues.AddRange(PlannedSeries.Points.Select(p => (float)p.Value));
        }
        if (Threshold != null)
        {
            allValues.Add((float)Threshold.Value);
        }

        return (allValues.Min(), allValues.Max());
    }

    private void DrawAreaFill(ICanvas canvas, RectF rect, float min, float range)
    {
        var points = Series!.Points;
        var fillPath = new PathF();
        var stepX = points.Count > 1 ? rect.Width / (points.Count - 1) : rect.Width;

        fillPath.MoveTo(rect.Left, rect.Bottom);
        for (var i = 0; i < points.Count; i++)
        {
            var x = rect.Left + (i * stepX);
            var y = rect.Bottom - (((float)points[i].Value - min) / range * rect.Height);
            fillPath.LineTo(x, y);
        }

        var lastX = rect.Left + ((points.Count - 1) * stepX);
        fillPath.LineTo(lastX, rect.Bottom);
        fillPath.Close();

        canvas.FillColor = ChartColorResolver.ResolveColor("SurfaceChart");
        canvas.FillPath(fillPath);
    }

    private void DrawThresholdLine(ICanvas canvas, RectF rect, float min, float range)
    {
        if (Threshold == null) { return; }

        var y = rect.Bottom - (((float)Threshold.Value - min) / range * rect.Height);
        canvas.StrokeColor = ChartColorResolver.ResolveColor("NegativeText");
        canvas.StrokeSize = 1f;
        canvas.StrokeDashPattern = new[] { 3f, 3f };
        canvas.DrawLine(rect.Left, y, rect.Right, y);
        canvas.StrokeDashPattern = null;
    }

    private void DrawPlannedLine(ICanvas canvas, RectF rect, float min, float range)
    {
        if (PlannedSeries == null || PlannedSeries.Points.Count == 0) { return; }

        var path = BuildLinePath(PlannedSeries.Points, rect, min, range);
        canvas.StrokeColor = ChartColorResolver.ResolveColor("TextSecondary");
        canvas.StrokeSize = 1.5f;
        canvas.StrokeDashPattern = new[] { 4f, 4f };
        canvas.DrawPath(path);
        canvas.StrokeDashPattern = null;
    }

    private void DrawActualLine(ICanvas canvas, RectF rect, float min, float range)
    {
        var path = BuildLinePath(Series!.Points, rect, min, range);
        canvas.StrokeColor = ChartColorResolver.ResolveColor("Indicator");
        canvas.StrokeSize = 2f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawPath(path);
    }

    private static PathF BuildLinePath(IReadOnlyList<ChartPoint> points, RectF rect, float min, float range)
    {
        var path = new PathF();
        var stepX = points.Count > 1 ? rect.Width / (points.Count - 1) : rect.Width;
        for (var i = 0; i < points.Count; i++)
        {
            var x = rect.Left + (i * stepX);
            var y = rect.Bottom - (((float)points[i].Value - min) / range * rect.Height);
            if (i == 0) { path.MoveTo(x, y); } else { path.LineTo(x, y); }
        }
        return path;
    }
}
