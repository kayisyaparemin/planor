using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// GK4'ün "aynı anda en fazla bir grafik" kuralını (GS22) ve kaydırılan hero'nun sınırlarını
/// (en fazla bir pager, iki sayfa, sayfa başına bir grafik) sabitleyen testler.
/// </summary>
public sealed class DesignHeroPagerTests
{
    private const string TwoPageHero = @"
<ContentPage xmlns:c=""clr-namespace:Mizan.App.Components"">
    <c:HeroPager>
        <c:HeroPage><c:AreaTrend /></c:HeroPage>
        <c:HeroPage><c:RingGauge /></c:HeroPage>
    </c:HeroPager>
</ContentPage>";

    [Fact]
    public void HeroPager_IkiSayfaBirerGrafik_AyniAndaBirGrafikSayilir()
    {
        var analysis = DesignBudgetAnalyzer.AnalyzeXaml(TwoPageHero);

        Assert.Equal(1, analysis.Charts);
        Assert.Equal(1, analysis.HeroPagers);
        Assert.Equal(2, analysis.HeroPages);
        Assert.True(analysis.IsValid, string.Join(Environment.NewLine, analysis.Violations));
    }

    [Fact]
    public void HeroPager_BirKartSayilir_SayfalariAyriKartOlmaz()
    {
        var analysis = DesignBudgetAnalyzer.AnalyzeXaml(TwoPageHero);

        Assert.Equal(1, analysis.Cards);
    }

    [Fact]
    public void HeroPager_DisindaGrafikVarsa_AyniAndaIkiGrafikOlur()
    {
        var xaml = TwoPageHero.Replace("</ContentPage>", "<c:Sparkline /></ContentPage>");

        var analysis = DesignBudgetAnalyzer.AnalyzeXaml(xaml);

        Assert.Equal(2, analysis.Charts);
        Assert.False(analysis.IsValid);
    }

    [Fact]
    public void HeroPage_IkiGrafikTasirsa_Kirmizidir()
    {
        var xaml = TwoPageHero.Replace("<c:AreaTrend />", "<c:AreaTrend /><c:Sparkline />");

        var analysis = DesignBudgetAnalyzer.AnalyzeXaml(xaml);

        Assert.Equal(2, analysis.Charts);
        Assert.False(analysis.IsValid);
    }

    [Fact]
    public void HeroPager_UcuncuSayfaEklenirse_Kirmizidir()
    {
        var xaml = TwoPageHero.Replace("</c:HeroPager>", "<c:HeroPage><c:Sparkline /></c:HeroPage></c:HeroPager>");

        var analysis = DesignBudgetAnalyzer.AnalyzeXaml(xaml);

        Assert.Equal(3, analysis.HeroPages);
        Assert.False(analysis.IsValid);
    }

    [Fact]
    public void HeroPager_IkinciPagerEklenirse_Kirmizidir()
    {
        var xaml = TwoPageHero.Replace("</ContentPage>", "<c:HeroPager><c:HeroPage /></c:HeroPager></ContentPage>");

        var analysis = DesignBudgetAnalyzer.AnalyzeXaml(xaml);

        Assert.Equal(2, analysis.HeroPagers);
        Assert.False(analysis.IsValid);
    }

    [Fact]
    public void HeroSayfaBeyani_KartBaslikSatirindanOkunur()
    {
        var body = "Label 11 / 28\nHero sayfa  2 / 2     kaydırılan kart\nGrafik        1 / 1     aynı anda";

        var declared = ScreenCardDocument.ParseDeclaredBudget(body);

        Assert.NotNull(declared);
        Assert.Equal(2, declared.HeroPageCount);
        Assert.Equal(1, declared.ChartCount);
    }

    [Fact]
    public void KaydirilanKartZemini_SurfaceChart_KontrastCiftleriTablodaDurur()
    {
        // GS24 (V3a Kapı C): kaydırılan kart konseptteki gibi tonlu zeminde durur; SurfaceHero koyu temada
        // metni taşımadığı için (GS22) ton SurfaceChart'tır. Kartın bütün metin ve çizgi rolleri tabloda olmalı.
        var pairs = DesignSystemDocument.GetContrastPairs();
        var pager = File.ReadAllText(Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components", "HeroPager.xaml"));

        Assert.Contains("BackgroundColor=\"{DynamicResource SurfaceChart}\"", pager, StringComparison.Ordinal);
        foreach (var text in new[] { "TextPrimary", "Indicator", "TextSecondary", "NegativeText", "PositiveText" })
        {
            Assert.Contains(pairs, p => p.TextToken == text && p.SurfaceToken == "SurfaceChart");
        }
    }
}
