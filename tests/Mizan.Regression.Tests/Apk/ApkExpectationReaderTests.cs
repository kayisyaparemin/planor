using Mizan.ApkVerifier;

namespace Mizan.Regression.Tests.Apk;

public sealed class ApkExpectationReaderTests
{
    [Fact]
    public void Csproj_BeklenenKimligiVerir()
    {
        var expectation = ApkExpectationReader.FromCsproj(ApkFixtures.Csproj, tagVersion: null, requireReleaseSignature: false);

        Assert.Equal(ApkFixtures.PrExpectation(), expectation);
    }

    [Fact]
    public void YayinEtiketi_VOnekiAtilir()
    {
        var expectation = ApkExpectationReader.FromCsproj(ApkFixtures.Csproj, "v0.1.0", requireReleaseSignature: true);

        Assert.Equal(ApkFixtures.ReleaseExpectation(), expectation);
    }

    [Theory]
    [InlineData("<ApplicationId>com.mizan.app</ApplicationId>")]
    [InlineData("<ApplicationVersion>1</ApplicationVersion>")]
    public void ZorunluOzellikYoksa_SessizceBosKalmaz(string removed)
    {
        var csproj = ApkFixtures.Csproj.Replace(removed, string.Empty, StringComparison.Ordinal);

        var error = Assert.Throws<InvalidDataException>(() => ApkExpectationReader.FromCsproj(csproj, null, false));

        Assert.Contains(removed[1..removed.IndexOf('>', StringComparison.Ordinal)], error.Message, StringComparison.Ordinal);
    }
}
