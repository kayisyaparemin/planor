using Microsoft.Maui.Graphics;
using Mizan.Presentation.Charts;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace Mizan.App.Charts;

/// <summary>
/// "Dönem boyunca bakiye nerede, plana göre nerede bitiyor?" sorusunu cevaplar.
/// Ana sayfada çizgi rota, bakiyenin dönemin hangi diliminde ne kadar olduğunu tek bakışta söylemiyordu;
/// sütunlar dönemi eşit dilimlere böler. Bugüne kadarkiler dolu, sonrakiler (tahmin) soluk, planın dönem sonu
/// seviyesinin altında kalan sütun olumsuz renkte. Bugünün sütunu üstünde "Bugün" etiketi taşır.
/// </summary>
public sealed class ColumnTrend : IDrawable
{
    // Tahmin sütunu dolu rengin bu kadar saydam hâlidir: iki temada da zeminin tonuna doğru söner.
    private const float AheadAlpha = 0.45f;
    // Sütun aralığı dilim genişliğinin bu payıdır (konseptte 48 px sütun, 12 px aralık).
    private const float GapRatio = 0.2f;
    private const float CornerRadius = 4f;
    private const float LabelPaddingX = 6f;
    private const float LabelPaddingY = 3f;
    private const float LabelGap = 4f;
    private const float FallbackLabelSize = 11f;
    private const string LabelFont = "OpenSansSemibold";

    /// <summary>Soldan sağa sütunlar (<see cref="ChartColumns"/>); boşsa çizim yoktur.</summary>
    public IReadOnlyList<ChartColumn>? Columns { get; set; }

    /// <summary>Planın dönem sonu seviyesi (ince yatay çizgi); altındaki sütun olumsuz renkte çizilir.</summary>
    public ChartThreshold? PlanLevel { get; set; }

    /// <summary>Bugünün sütununun üstündeki etiket; verilmezse etiket çizilmez.</summary>
    public string? TodayLabel { get; set; }

    /// <summary>
    /// Çizim yüzeyine sütunlu rota grafiğini çizer.
    /// </summary>
    /// <param name="canvas">Çizim tuvali.</param>
    /// <param name="dirtyRect">Çizim alanı sınırları.</param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (canvas == null || dirtyRect.Width <= 0 || dirtyRect.Height <= 0) { return; }
        if (Columns is not { Count: > 0 } columns) { return; }

        var labelSize = ResolveSize("TypeEyebrow", FallbackLabelSize);
        var pillHeight = labelSize + (2 * LabelPaddingY);
        var topReserved = pillHeight + LabelGap + 2f;
        var bottom = dirtyRect.Bottom;
        var maxBarHeight = dirtyRect.Height - topReserved;
        var minBarHeight = dirtyRect.Height * 0.22f;

        var (minVal, maxVal) = ValueBounds(columns);
        float HeightOf(decimal value) => maxVal <= minVal
            ? (maxBarHeight + minBarHeight) / 2f
            : minBarHeight + ((float)((value - minVal) / (maxVal - minVal)) * (maxBarHeight - minBarHeight));

        DrawPlanLine(canvas, dirtyRect, bottom, HeightOf);
        DrawColumns(canvas, dirtyRect, columns, bottom, HeightOf, labelSize);
    }

    private void DrawPlanLine(ICanvas canvas, RectF dirtyRect, float bottom, Func<decimal, float> heightOf)
    {
        if (PlanLevel == null) { return; }

        var planY = bottom - heightOf(PlanLevel.Value);
        canvas.StrokeColor = ChartColorResolver.ResolveColor("TextSecondary");
        canvas.StrokeSize = 1f;
        canvas.DrawLine(dirtyRect.Left, planY, dirtyRect.Right, planY);
    }

    private void DrawColumns(
        ICanvas canvas,
        RectF dirtyRect,
        IReadOnlyList<ChartColumn> columns,
        float bottom,
        Func<decimal, float> heightOf,
        float labelSize)
    {
        var gap = dirtyRect.Width / columns.Count * GapRatio;
        var width = (dirtyRect.Width - (gap * (columns.Count - 1))) / columns.Count;

        for (var i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var x = dirtyRect.Left + (i * (width + gap));
            var h = heightOf(col.Value);
            var top = bottom - h;

            var isBehind = PlanLevel != null && col.Value < PlanLevel.Value;
            var color = isBehind
                ? ChartColorResolver.ResolveColor("NegativeText")
                : ChartColorResolver.ResolveColor("Indicator");

            canvas.FillColor = col.IsAhead ? color.WithAlpha(AheadAlpha) : color;

            var radius = Math.Min(CornerRadius, width / 2f);
            canvas.FillRoundedRectangle(x, top, width, h, radius, radius, 0f, 0f);

            if (!string.IsNullOrEmpty(TodayLabel) && col.IsToday)
            {
                DrawTodayLabel(canvas, dirtyRect, x + (width / 2f), top, labelSize);
            }
        }
    }

    private (decimal Min, decimal Max) ValueBounds(IReadOnlyList<ChartColumn> columns)
    {
        var values = columns.Select(c => c.Value).ToList();
        if (PlanLevel != null) { values.Add(PlanLevel.Value); }

        var min = values.Min();
        var max = values.Max();

        if (min >= 0m)
        {
            return (0m, max > 0m ? max : 1m);
        }

        return (min, max > min ? max : min + 1m);
    }

    private void DrawTodayLabel(ICanvas canvas, RectF bounds, float centerX, float columnTop, float fontSize)
    {
        var font = new Microsoft.Maui.Graphics.Font(LabelFont);
        var text = canvas.GetStringSize(TodayLabel!, font, fontSize);
        var width = text.Width + (2 * LabelPaddingX);
        var height = fontSize + (2 * LabelPaddingY);
        var left = Math.Clamp(centerX - (width / 2f), bounds.Left, Math.Max(bounds.Left, bounds.Right - width));
        var pill = new RectF(left, Math.Max(bounds.Top, columnTop - LabelGap - height), width, height);

        canvas.FillColor = ChartColorResolver.ResolveColor("Indicator");
        canvas.FillRoundedRectangle(pill, height / 2f);

        canvas.Font = font;
        canvas.FontSize = fontSize;
        canvas.FontColor = ChartColorResolver.ResolveColor("TextOnAction");
        canvas.DrawString(TodayLabel!, pill, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    // Etiketin puntosu tip skalasından gelir (GK2); headless ortamda kaynak yoksa skaladaki değer.
    private static float ResolveSize(string tokenName, float fallback) =>
        MauiApp.Current?.Resources is { } resources &&
        resources.TryGetValue(tokenName, out var value) && value is double size
            ? (float)size
            : fallback;
}
