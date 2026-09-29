using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// GK4, GK5 ve GK9 kurallarını zorlayarak ekranların görsel ve cümle
/// bütçesini, kart zorunluluğunu mimari olarak denetleyen test kalkanı.
/// </summary>
public sealed class DesignBudgetTests
{
    [Fact]
    public void Sayfa_GorselButceyiAsamaz()
    {
        var pages = XamlSources.GetAllDocuments()
            .Where(d => d.FileName.EndsWith("Page.xaml", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var failures = new List<string>();
        foreach (var page in pages)
        {
            var analysis = DesignBudgetAnalyzer.AnalyzeXaml(page.Content);
            if (!analysis.IsValid)
            {
                failures.Add($"{page.FileName}:{Environment.NewLine}" + string.Join(Environment.NewLine, analysis.Violations));
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Sayfa_CumleButcesiniAsamaz()
    {
        var pages = XamlSources.GetAllDocuments()
            .Where(d => d.FileName.EndsWith("Page.xaml", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var failures = new List<string>();
        foreach (var page in pages)
        {
            var analysis = DesignBudgetAnalyzer.AnalyzeXaml(page.Content);
            if (analysis.Sentences > DesignBudgetAnalyzer.MaxSentences)
            {
                failures.Add($"{page.FileName}: Cümle sayısı sınırı aşıldı ({analysis.Sentences} > {DesignBudgetAnalyzer.MaxSentences})");
            }
        }

        var stringsPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Resources", "Strings");
        if (Directory.Exists(stringsPath))
        {
            var files = Directory.GetFiles(stringsPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var matches = Regex.Matches(content, @"""(?<key>[A-Za-z0-9_]+)""\s*:\s*""(?<val>[^""]*)""");
                foreach (Match m in matches)
                {
                    failures.AddRange(DesignBudgetAnalyzer.ValidateStringEntry(m.Groups["key"].Value, m.Groups["val"].Value));
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void HerSayfanin_EkranKarti_Var()
    {
        var cards = ScreenCardDocument.GetAllCards();
        var pages = XamlSources.GetAllDocuments()
            .Where(d => d.FileName.EndsWith("Page.xaml", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var failures = new List<string>();
        foreach (var page in pages)
        {
            var hasCard = cards.Any(c => c.Body.Contains(page.FileName, StringComparison.OrdinalIgnoreCase) ||
                                         page.Content.Contains(c.Key, StringComparison.OrdinalIgnoreCase));
            if (!hasCard)
            {
                failures.Add($"Sayfa '{page.FileName}' için docs/EKRAN-KARTLARI.md belgesinde kart bulunamadı.");
            }
        }

        foreach (var card in cards.Where(c => !c.IsHeadlessException && c.IsFilled))
        {
            var hasPage = pages.Any(p => card.Body.Contains(p.FileName, StringComparison.OrdinalIgnoreCase));
            if (!hasPage && !card.Body.Contains("teklif", StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"Kart '{card.Key}' ({card.Title}) için diskte karşılık gelen sayfa dosyası bulunamadı.");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void GorselButceAnalizcisi_SinirAsimlarini_Yakalar()
    {
        var violatingXaml = @"
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:c=""clr-namespace:Mizan.App.Components"">
    <Label Text=""1"" Style=""{StaticResource HeroFigure}"" />
    <Label Text=""2"" Style=""{StaticResource HeroFigure}"" />
    <c:HeroInputCard />
    <c:InfoBanner />
    <c:SummaryCard /><c:SummaryCard /><c:SummaryCard /><c:SummaryCard /><c:SummaryCard />
    <c:ChartCard /><c:ChartCard />
    <c:NavRow /><c:NavRow /><c:NavRow /><c:NavRow /><c:NavRow /><c:NavRow />
    " + string.Join("\n", Enumerable.Range(1, 29).Select(i => $"<Label Text=\"{i}\" />")) + @"
    <Label Text=""{x:Static s:Strings.Cumle_Bir}"" />
    <Label Text=""{x:Static s:Strings.Cumle_Iki}"" />
    <Label Text=""{x:Static s:Strings.Cumle_Uc}"" />
    <Label Text=""{x:Static s:Strings.Cumle_Dort}"" />
</ContentPage>";

        var analysis = DesignBudgetAnalyzer.AnalyzeXaml(violatingXaml);

        Assert.False(analysis.IsValid);
        Assert.Equal(2, analysis.HeroFigures);
        Assert.Equal(2, analysis.HeroSurfaces);
        Assert.Equal(8, analysis.Cards);
        Assert.Equal(2, analysis.Charts);
        Assert.Equal(6, analysis.NavRows);
        Assert.Equal(35, analysis.Labels);
        Assert.Equal(4, analysis.Sentences);
        Assert.Equal(7, analysis.Violations.Count);
    }

    [Fact]
    public void CumleButcesi_KarakterSinirlarini_Zorlar()
    {
        var longEtiket = new string('A', 25);
        var longCumle = new string('B', 91);
        var longAksiyon = new string('C', 29);
        var longDurum = new string('D', 91);

        var v1 = DesignBudgetAnalyzer.ValidateStringEntry("Etiket_Ad", longEtiket);
        var v2 = DesignBudgetAnalyzer.ValidateStringEntry("Cumle_Aciklama", longCumle);
        var v3 = DesignBudgetAnalyzer.ValidateStringEntry("Aksiyon_Guncelle", longAksiyon);
        var v4 = DesignBudgetAnalyzer.ValidateStringEntry("Bos_Durum", longDurum);
        var v5 = DesignBudgetAnalyzer.ValidateStringEntry("Hata_Durum", longDurum);

        Assert.Single(v1);
        Assert.Single(v2);
        Assert.Single(v3);
        Assert.Single(v4);
        Assert.Single(v5);
    }

    [Fact]
    public void EkranKartlari_Belgesi_EnAz12KartVeIstisnalariTasar()
    {
        var cards = ScreenCardDocument.GetAllCards();

        Assert.True(cards.Count >= 12, $"Beklenen en az 12 kart bulunamadı, mevcut: {cards.Count}");
        Assert.Contains(cards, c => c.Key == "EK-V0" && c.IsHeadlessException);
        Assert.Contains(cards, c => c.Key == "EK-V2" && c.IsHeadlessException);
        Assert.Contains(cards, c => c.Key == "EK-V3" && c.IsFilled);

        var v3Card = cards.First(c => c.Key == "EK-V3");
        var declaredBudget = ScreenCardDocument.ParseDeclaredBudget(v3Card.Body);
        Assert.NotNull(declaredBudget);
        Assert.True(declaredBudget.LabelCount <= DesignBudgetAnalyzer.MaxLabels);
        Assert.True(declaredBudget.HeroCount <= DesignBudgetAnalyzer.MaxHeroFigures);
        Assert.True(declaredBudget.HeroSurfaceCount <= DesignBudgetAnalyzer.MaxHeroSurfaces);
        Assert.True(declaredBudget.CardCount <= DesignBudgetAnalyzer.MaxCards);
        Assert.True(declaredBudget.ChartCount <= DesignBudgetAnalyzer.MaxCharts);
        Assert.True(declaredBudget.HeroPageCount <= DesignBudgetAnalyzer.MaxHeroPages);
        Assert.True(declaredBudget.NavRowCount <= DesignBudgetAnalyzer.MaxNavRows);
        Assert.True(declaredBudget.SentenceCount <= DesignBudgetAnalyzer.MaxSentences);
    }
}
