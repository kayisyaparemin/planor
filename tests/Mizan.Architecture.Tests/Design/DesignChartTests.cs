using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// Kural GK7 (grafik yalnız dört primitiften biri, verisi ham seri gelir) ve
/// grafik mimari sınırlarını denetleyen test kalkanı.
/// </summary>
public sealed class DesignChartTests
{
    private static readonly string[] AllowedChartPrimitives =
    [
        "Sparkline",
        "AreaTrend",
        "StackedBar",
        "RingGauge"
    ];

    private static readonly Dictionary<string, string> PrimitiveQuestions = new()
    {
        ["Sparkline"] = "Yön ne, yukarı mı aşağı mı?",
        ["AreaTrend"] = "Bakiye nereye gidiyor, plana ve eşiğe göre neredeyim?",
        ["StackedBar"] = "Bu dönem neyden oluşuyor?",
        ["RingGauge"] = "Ne kadarı tamamlandı, geçen süreye göre önde miyiz geride mi?"
    };

    [Fact]
    public void Grafik_YalnizPrimitiflerden_IzinVerilenDortPrimitifVar()
    {
        var chartsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts");
        Assert.True(Directory.Exists(chartsDir), "src/Mizan.App/Charts dizini bulunamadı.");

        var csFiles = Directory.GetFiles(chartsDir, "*.cs")
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();

        // 4 primitif ile ChartColorResolver ve ChartScale yardımcıları dışında dosya bulunamaz
        var allowedFiles = AllowedChartPrimitives.Concat(["ChartColorResolver", "ChartScale"]).ToList();

        foreach (var file in csFiles)
        {
            Assert.Contains(file, allowedFiles);
        }

        foreach (var primitive in AllowedChartPrimitives)
        {
            Assert.Contains(primitive, csFiles);
        }
    }

    [Fact]
    public void Grafikler_IDrawable_Uygular()
    {
        var chartsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts");
        Assert.True(Directory.Exists(chartsDir), "src/Mizan.App/Charts dizini bulunamadı.");

        foreach (var primitive in AllowedChartPrimitives)
        {
            var filePath = Path.Combine(chartsDir, $"{primitive}.cs");
            Assert.True(File.Exists(filePath), $"{primitive}.cs dosyası bulunamadı.");

            var content = File.ReadAllText(filePath);
            Assert.Contains(": IDrawable", content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Grafikler_Summary_CevapladigiSoruyuTasar()
    {
        var chartsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts");
        Assert.True(Directory.Exists(chartsDir), "src/Mizan.App/Charts dizini bulunamadı.");

        foreach (var (primitive, question) in PrimitiveQuestions)
        {
            var filePath = Path.Combine(chartsDir, $"{primitive}.cs");
            var content = File.ReadAllText(filePath);

            Assert.Contains(question, content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Grafikler_K3_SatirSinirlarini_Asamaz()
    {
        var chartsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts");
        if (!Directory.Exists(chartsDir)) { return; }

        var files = Directory.GetFiles(chartsDir, "*.cs");
        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file).Length;
            Assert.True(lines <= 200, $"{Path.GetFileName(file)} dosyası {lines} satır (≤ 200 olmalı).");
        }
    }

    [Fact]
    public void GrafikModelleri_MizanPresentationAltinda_Ve_MAUIReferansiYok()
    {
        var modelsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.Presentation", "Charts");
        Assert.True(Directory.Exists(modelsDir), "src/Mizan.Presentation/Charts dizini bulunamadı.");

        var expectedModels = new[] { "ChartPoint.cs", "ChartSeries.cs", "ChartThreshold.cs", "ChartCategory.cs" };
        var actualFiles = Directory.GetFiles(modelsDir, "*.cs").Select(Path.GetFileName).ToList();

        foreach (var expected in expectedModels)
        {
            Assert.Contains(expected, actualFiles);
        }

        foreach (var file in Directory.GetFiles(modelsDir, "*.cs"))
        {
            var content = File.ReadAllText(file);
            Assert.DoesNotContain("Microsoft.Maui", content, StringComparison.Ordinal);
            Assert.DoesNotContain("IDrawable", content, StringComparison.Ordinal);
            Assert.DoesNotContain("using Microsoft.Maui.Graphics", content, StringComparison.Ordinal);

            var lines = File.ReadAllLines(file).Length;
            Assert.True(lines <= 200, $"{Path.GetFileName(file)} {lines} satır (≤ 200 olmalı).");
        }
    }

    [Fact]
    public void Grafikler_HamRenkLiterali_Iceremez()
    {
        var chartsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts");
        if (!Directory.Exists(chartsDir)) { return; }

        // ChartColorResolver renk fallback'leri taşır; 4 grafik primitifi ise ColorResolver kullanmalıdır
        var hexRegex = new Regex(@"#[0-9a-fA-F]{6,8}");

        foreach (var primitive in AllowedChartPrimitives)
        {
            var filePath = Path.Combine(chartsDir, $"{primitive}.cs");
            var content = File.ReadAllText(filePath);

            Assert.DoesNotMatch(hexRegex, content);
        }
    }

    [Fact]
    public void Grafikler_CizimMetodu_Uygulanmis_NotImplementedFirlatmaz()
    {
        var chartsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts");
        Assert.True(Directory.Exists(chartsDir), "src/Mizan.App/Charts dizini bulunamadı.");

        foreach (var primitive in AllowedChartPrimitives)
        {
            var filePath = Path.Combine(chartsDir, $"{primitive}.cs");
            var content = File.ReadAllText(filePath);

            Assert.DoesNotContain("NotImplementedException", content);
        }
    }

    [Fact]
    public void Grafikler_GerekliAlanlari_Tasar()
    {
        var chartsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts");

        var sparklineContent = File.ReadAllText(Path.Combine(chartsDir, "Sparkline.cs"));
        Assert.Contains("ChartSeries? Series", sparklineContent, StringComparison.Ordinal);

        var areaTrendContent = File.ReadAllText(Path.Combine(chartsDir, "AreaTrend.cs"));
        Assert.Contains("ChartSeries? Series", areaTrendContent, StringComparison.Ordinal);
        Assert.Contains("ChartSeries? PlannedSeries", areaTrendContent, StringComparison.Ordinal);
        Assert.Contains("ChartThreshold? Threshold", areaTrendContent, StringComparison.Ordinal);

        var stackedBarContent = File.ReadAllText(Path.Combine(chartsDir, "StackedBar.cs"));
        Assert.Contains("IReadOnlyList<ChartCategory>? Categories", stackedBarContent, StringComparison.Ordinal);

        var ringGaugeContent = File.ReadAllText(Path.Combine(chartsDir, "RingGauge.cs"));
        Assert.Contains("decimal Ratio", ringGaugeContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Grafikler_CizimDavranisi_GerekliGeometrileriCagirir()
    {
        var chartsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Charts");

        var sparkline = File.ReadAllText(Path.Combine(chartsDir, "Sparkline.cs"));
        Assert.Contains("ChartColorResolver.ResolveColor(\"Indicator\")", sparkline, StringComparison.Ordinal);
        Assert.Contains("DrawPath", sparkline, StringComparison.Ordinal);

        var areaTrend = File.ReadAllText(Path.Combine(chartsDir, "AreaTrend.cs"));
        Assert.Contains("ChartColorResolver.ResolveColor(\"SurfaceChart\")", areaTrend, StringComparison.Ordinal);
        Assert.Contains("ChartColorResolver.ResolveColor(\"NegativeText\")", areaTrend, StringComparison.Ordinal);
        Assert.Contains("StrokeDashPattern", areaTrend, StringComparison.Ordinal);

        var stackedBar = File.ReadAllText(Path.Combine(chartsDir, "StackedBar.cs"));
        Assert.Contains("FillRoundedRectangle", stackedBar, StringComparison.Ordinal);

        var ringGauge = File.ReadAllText(Path.Combine(chartsDir, "RingGauge.cs"));
        Assert.Contains("DrawArc", ringGauge, StringComparison.Ordinal);
        Assert.Contains("ChartColorResolver.ResolveColor(\"BorderSubtle\")", ringGauge, StringComparison.Ordinal);
    }
}
