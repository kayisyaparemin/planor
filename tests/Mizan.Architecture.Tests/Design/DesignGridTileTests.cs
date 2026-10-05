using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// 12 Dönem ve Simülatör ekranlarındaki 3x4 dönem karolarının dar ekranlarda kırılmasını
/// ve taşmasını engelleyen görsel kalkan testleri (GK2, GK3, GK4).
/// </summary>
public sealed class DesignGridTileTests
{
    private static readonly string[] PagesWithDonemKarosu =
    [
        "FuturePeriodsPage.xaml",
        "SimulatorPage.xaml"
    ];

    [Fact]
    public void DonemKarosu_HucreDolgusuVeYaziBoyutu_GuvenliOlcudeVeNoWrapOlmali()
    {
        var failures = new List<string>();

        foreach (var pageName in PagesWithDonemKarosu)
        {
            var doc = XamlSources.FindDocument(pageName);
            Assert.True(doc is not null, $"{pageName} bulunamadı.");

            // DonemKarosu şablonunu ayıkla
            var templateMatch = Regex.Match(
                doc.Content,
                @"<DataTemplate\s+x:Key=""DonemKarosu"".*?</DataTemplate>",
                RegexOptions.Singleline);

            Assert.True(templateMatch.Success, $"{pageName} içinde DonemKarosu şablonu bulunamadı.");
            var templateContent = templateMatch.Value;

            // Kök dolgusu Space2 olmalı (Space3 dar ekranda taşmaya yol açar)
            if (!templateContent.Contains("Padding=\"{StaticResource Space2}\""))
            {
                failures.Add($"{pageName}: DonemKarosu Padding=\"{{StaticResource Space2}}\" olmalıdır.");
            }

            // Tutar etiketi TypeBody olmalı (TypeFigure 6 haneli sayılarda taşar)
            if (!templateContent.Contains("FontSize=\"{StaticResource TypeBody}\""))
            {
                failures.Add($"{pageName}: DonemKarosu tutar etiketi FontSize=\"{{StaticResource TypeBody}}\" olmalıdır.");
            }

            // Tutar etiketi LineBreakMode="NoWrap" taşımalı (rakamların bölünmesini engeller)
            if (!templateContent.Contains("LineBreakMode=\"NoWrap\""))
            {
                failures.Add($"{pageName}: DonemKarosu tutar etiketi LineBreakMode=\"NoWrap\" taşımalıdır.");
            }
        }

        Assert.True(failures.Count == 0, string.Join("\n", failures));
    }

    [Fact]
    public void ParaConverter_TutarIleSimgeArasinda_BolunemezBoslukKullanir()
    {
        var converterPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Converters", "ParaConverter.cs");
        Assert.True(File.Exists(converterPath), "ParaConverter.cs bulunamadı.");

        var content = File.ReadAllText(converterPath);

        // Tutardan sonra bölünemez boşluk (\u00A0) kullanılmalıdır
        Assert.Contains(@"\u00A0₺", content);
    }
}
