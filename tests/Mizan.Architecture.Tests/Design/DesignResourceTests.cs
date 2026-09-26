using System.Text.RegularExpressions;
using Mizan.Architecture.Tests.Design.Support;
using Xunit;

namespace Mizan.Architecture.Tests.Design;

/// <summary>
/// XAML'deki her <c>{StaticResource X}</c> anahtarının bir kaynak sözlüğünde tanımlı olduğunu denetler.
/// XAML çalışma anında yüklendiği için eksik anahtar derlemeyi kırmaz; sayfa ilk açıldığında uygulamayı
/// çökertir (V3'te ana sayfa, olmayan <c>PrimaryButton</c> stili yüzünden profil seçilince çöküyordu).
/// </summary>
public sealed class DesignResourceTests
{
    private static readonly Regex KeyDefinition = new(@"x:Key=""([^""]+)""");
    private static readonly Regex StaticResourceUse = new(@"\{StaticResource\s+([A-Za-z0-9_]+)\s*\}");

    [Fact]
    public void Xaml_StaticResource_TanimliAnahtaraBakar()
    {
        var docs = XamlSources.GetAllDocuments();
        var defined = docs
            .SelectMany(d => KeyDefinition.Matches(d.Content).Select(m => m.Groups[1].Value))
            .ToHashSet(StringComparer.Ordinal);

        var missing = docs
            .SelectMany(d => StaticResourceUse.Matches(d.Content)
                .Select(m => m.Groups[1].Value)
                .Where(key => !defined.Contains(key))
                .Select(key => $"{d.FileName}: {key}"))
            .Distinct()
            .ToList();

        Assert.True(missing.Count == 0, "Tanımsız StaticResource anahtarı:\n" + string.Join("\n", missing));
    }
}
