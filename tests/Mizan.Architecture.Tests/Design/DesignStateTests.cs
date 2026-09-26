using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// Durum blokları (StateBlock, SkeletonBlock) ve iskelet yükleme deseninin
/// tasarım sistemi kurallarını (GS14, GK1, GK3, GK8, § Durumlar) denetleyen test kalkanı.
/// </summary>
public sealed class DesignStateTests
{
    [Fact]
    public void DurumBloklari_Spinner_ActivityIndicator_Yasak()
    {
        var componentsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components");
        var xamlFiles = Directory.GetFiles(componentsDir, "*.xaml", SearchOption.AllDirectories);

        var violations = new List<string>();
        foreach (var file in xamlFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("<ActivityIndicator", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(Path.GetFileName(file));
            }
        }

        Assert.True(
            violations.Count == 0,
            $"Tasarım sistemi § Durumlar uyarınca spinner kullanımı yasaktır ('Spinner yok; yerleşim zıplamaz'): {string.Join(", ", violations)}");
    }

    [Fact]
    public void DurumBloklari_SkeletonBlock_SurfaceSunkenKullanir()
    {
        var xamlPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components", "SkeletonBlock.xaml");
        Assert.True(File.Exists(xamlPath), "SkeletonBlock.xaml dosyası bulunamadı.");

        var content = File.ReadAllText(xamlPath);
        Assert.Contains("{DynamicResource SurfaceSunken}", content, StringComparison.Ordinal);
    }

    [Fact]
    public void DurumBloklari_Presentation_ScreenState_DortDurumuTasar()
    {
        var type = typeof(Mizan.Presentation.Models.ScreenState);
        var names = Enum.GetNames(type);

        Assert.Equal(4, names.Length);
        Assert.Contains("Loading", names);
        Assert.Contains("Content", names);
        Assert.Contains("Empty", names);
        Assert.Contains("Error", names);
    }

    [Fact]
    public void DurumBloklari_StateBlock_Ve_SkeletonBlock_K3_Ve_K8_KurallarinaUyar()
    {
        var componentsDir = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components");
        string[] targetFiles = ["StateBlock", "SkeletonBlock"];

        foreach (var target in targetFiles)
        {
            var csPath = Path.Combine(componentsDir, $"{target}.xaml.cs");
            Assert.True(File.Exists(csPath), $"{target}.xaml.cs bulunamadı.");

            var lines = File.ReadAllLines(csPath);
            Assert.True(lines.Length <= 200, $"{target}.xaml.cs satır sayısı {lines.Length} > 200 (K3 ihlali).");

            var content = string.Join(Environment.NewLine, lines);
            Assert.Matches(new Regex(@"///\s*<summary>\s*\r?\n\s*///\s*[^<]+", RegexOptions.Multiline), content);
        }
    }

    [Fact]
    public void DurumBloklari_StateBlock_HataDurumunda_NegativeText_Kullanir()
    {
        var csPath = Path.Combine(SolutionPaths.SourceDirectory, "Mizan.App", "Components", "StateBlock.xaml.cs");
        Assert.True(File.Exists(csPath), "StateBlock.xaml.cs bulunamadı.");

        var content = File.ReadAllText(csPath);
        Assert.Contains("\"NegativeText\"", content, StringComparison.Ordinal);
    }
}
