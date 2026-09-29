using Microsoft.Maui.Graphics;
using Mizan.Presentation.Charts;

namespace Mizan.App.Charts;

/// <summary>
/// "Bakiye nereye gidiyor, plana ve eşiğe göre neredeyim?" sorusunu cevaplar.
/// Dönem sonu bakiyesi 12 dönem boyunca sıfırın altına düşüyorsa kullanıcının bunu tek bakışta
/// görmesi gerekiyor; ana sayfada ise girilen bakiyelerden dönem sonu tahminine giden rota,
/// bugünün yeri ve planın dönem sonu aynı çizimde durur. Yatay eksen tarihtir: nokta aralığı
/// gün sayısını yansıtır.
/// </summary>
public sealed class AreaTrend : IDrawable
{
    private static readonly float[] PlannedDash = [4f, 4f];
    private static readonly float[] ThresholdDash = [3f, 3f];
    private static readonly float[] PlanLevelDash = [1f, 3f];

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
    /// Son gözlemden dönem sonu tahminine kesikli devam. İlk noktası son gözlemdir; çizgi
    /// <see cref="Series"/>'in bittiği yerden başlasın diye.
    /// </summary>
    public ChartSeries? ProjectionSeries { get; set; }

    /// <summary>
    /// "Bugün" günü; verilirse o tarihte dikey ince çizgi çizilir. Serilerin tarih aralığı dışındaysa çizilmez.
    /// </summary>
    public DateOnly? Today { get; set; }

    /// <summary>
    /// Planın dönem sonu seviyesi (ince yatay çizgi); plana göre önde mi geride mi olunduğunu gösterir.
    /// </summary>
    public ChartThreshold? PlanLevel { get; set; }

    /// <summary>
    /// Çizim yüzeyine alan dolgulu trend grafiğini çizer.
    /// </summary>
    /// <param name="canvas">Çizim tuvali.</param>
    /// <param name="dirtyRect">Çizim alanı sınırları.</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (canvas == null || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) { return; }
        if (Series == null || Series.Points.Count == 0) { return; }

        var scale = BuildScale(dirtyRect);

        DrawAreaFill(canvas, scale);
        DrawThresholdLine(canvas, scale, dirtyRect);
        DrawPlanLevel(canvas, scale, dirtyRect);
        DrawPlannedLine(canvas, scale);
        DrawProjection(canvas, scale);
        DrawActualLine(canvas, scale);
        DrawTodayLine(canvas, scale, dirtyRect);
    }

    private ChartScale BuildScale(RectF rect)
    {
        var points = new List<ChartPoint>(Series!.Points);
        if (PlannedSeries != null) { points.AddRange(PlannedSeries.Points); }
        if (ProjectionSeries != null) { points.AddRange(ProjectionSeries.Points); }

        var levels = points.Select(p => p.Value).ToList();
        if (Threshold != null) { levels.Add(Threshold.Value); }
        if (PlanLevel != null) { levels.Add(PlanLevel.Value); }

        return new ChartScale(rect, points.Min(p => p.Date), points.Max(p => p.Date), levels.Min(), levels.Max());
    }

    private void DrawAreaFill(ICanvas canvas, ChartScale scale)
    {
        canvas.FillColor = ChartColorResolver.ResolveColor("SurfaceChart");
        canvas.FillPath(scale.AreaPath(Series!.Points));
    }

    private void DrawThresholdLine(ICanvas canvas, ChartScale scale, RectF rect)
    {
        if (Threshold == null) { return; }

        var y = scale.Y(Threshold.Value);
        canvas.StrokeColor = ChartColorResolver.ResolveColor("NegativeText");
        canvas.StrokeSize = 1f;
        canvas.StrokeDashPattern = ThresholdDash;
        canvas.DrawLine(rect.Left, y, rect.Right, y);
        canvas.StrokeDashPattern = null;
    }

    private void DrawPlanLevel(ICanvas canvas, ChartScale scale, RectF rect)
    {
        if (PlanLevel == null) { return; }

        var y = scale.Y(PlanLevel.Value);
        canvas.StrokeColor = ChartColorResolver.ResolveColor("TextSecondary");
        canvas.StrokeSize = 1f;
        canvas.StrokeDashPattern = PlanLevelDash;
        canvas.DrawLine(rect.Left, y, rect.Right, y);
        canvas.StrokeDashPattern = null;
    }

    private void DrawPlannedLine(ICanvas canvas, ChartScale scale)
    {
        if (PlannedSeries == null || PlannedSeries.Points.Count == 0) { return; }

        canvas.StrokeColor = ChartColorResolver.ResolveColor("TextSecondary");
        canvas.StrokeSize = 1.5f;
        canvas.StrokeDashPattern = PlannedDash;
        canvas.DrawPath(scale.LinePath(PlannedSeries.Points));
        canvas.StrokeDashPattern = null;
    }

    private void DrawProjection(ICanvas canvas, ChartScale scale)
    {
        if (ProjectionSeries == null || ProjectionSeries.Points.Count == 0) { return; }

        canvas.StrokeColor = ChartColorResolver.ResolveColor("Indicator");
        canvas.StrokeSize = 2f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeDashPattern = PlannedDash;
        canvas.DrawPath(scale.LinePath(ProjectionSeries.Points));
        canvas.StrokeDashPattern = null;
    }

    private void DrawActualLine(ICanvas canvas, ChartScale scale)
    {
        canvas.StrokeColor = ChartColorResolver.ResolveColor("Indicator");
        canvas.StrokeSize = 2f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawPath(scale.LinePath(Series!.Points));
    }

    private void DrawTodayLine(ICanvas canvas, ChartScale scale, RectF rect)
    {
        if (Today is not { } today || !scale.Contains(today)) { return; }

        var x = scale.X(today);
        canvas.StrokeColor = ChartColorResolver.ResolveColor("TextSecondary");
        canvas.StrokeSize = 1f;
        canvas.DrawLine(x, rect.Top, x, rect.Bottom);
    }
}
