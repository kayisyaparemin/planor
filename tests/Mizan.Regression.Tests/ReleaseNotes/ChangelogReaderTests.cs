using Mizan.ReleaseNotes;

namespace Mizan.Regression.Tests.ReleaseNotes;

public sealed class ChangelogReaderTests
{
    [Fact]
    public void Read_SurumVarsa_SurumTarihVeGovdeyiVerir()
    {
        var note = ChangelogReader.Read(ChangelogFixtures.Valid, "0.1.0");

        Assert.Equal("0.1.0", note.Version);
        Assert.Equal(new DateOnly(2026, 10, 5), note.ReleaseDate);
        Assert.StartsWith("### Eklendi", note.Body);
        Assert.EndsWith("- İlk sürümün düzeltmesi.", note.Body);
    }

    [Fact]
    public void Read_KomsuSurumunMetniniGovdeyeKatmaz()
    {
        var note = ChangelogReader.Read(ChangelogFixtures.Valid, "0.1.0");

        Assert.DoesNotContain("İkinci sürümün", note.Body);
        Assert.DoesNotContain("Eski sürümün", note.Body);
        Assert.DoesNotContain("## [", note.Body);
    }

    [Fact]
    public void Read_AltBasliklariVeMaddeleriGovdedeKorur()
    {
        var note = ChangelogReader.Read(ChangelogFixtures.Valid, "0.1.0");

        Assert.Contains("### Düzeltildi", note.Body);
        Assert.Contains("- İlk sürümün ikinci maddesi.", note.Body);
    }

    [Fact]
    public void Read_EtiketinVOnekiniKabulEder()
    {
        var note = ChangelogReader.Read(ChangelogFixtures.Valid, "v0.2.0");

        Assert.Equal("0.2.0", note.Version);
        Assert.Contains("İkinci sürümün maddesi.", note.Body);
    }

    [Fact]
    public void Read_CrLfSatirSonlarini_LfOlarakVerir()
    {
        var windows = ChangelogFixtures.Valid.Replace("\n", "\r\n");

        var note = ChangelogReader.Read(windows, "0.1.0");

        Assert.DoesNotContain('\r', note.Body);
        Assert.Contains("- İlk sürümün birinci maddesi.\n- İlk sürümün ikinci maddesi.", note.Body);
    }

    [Fact]
    public void Read_DosyaninSonBolumunu_DosyaSonunaKadarOkur()
    {
        var note = ChangelogReader.Read(ChangelogFixtures.Valid, "0.0.1");

        Assert.Equal("- Eski sürümün maddesi.", note.Body);
    }

    [Theory]
    [InlineData("0.3.0")]
    [InlineData("0.9.0")]
    [InlineData("10.1.0")]
    [InlineData("0.1.10")]
    public void Read_SurumunBolumuYoksa_IhlalVerirVeSurumuAnar(string version)
    {
        // 10.1.0 ve 0.1.10, 0.1.0'ın öneki ya da soneki olarak eşleşmemeli.
        var exception = Assert.Throws<InvalidDataException>(() => ChangelogReader.Read(ChangelogFixtures.Valid, version));

        Assert.Contains(version, exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Yayınlanmamış")]
    [InlineData("1.0")]
    [InlineData("0.1.00x")]
    public void Read_SurumBicimiYanlissa_ArgumentExceptionFirlatir(string version)
    {
        Assert.Throws<ArgumentException>(() => ChangelogReader.Read(ChangelogFixtures.Valid, version));
    }

    [Fact]
    public void Read_BolumBossa_IhlalVerir()
    {
        var changelog = ChangelogFixtures.With("## [0.1.0] - 2026-10-05", string.Empty);

        Assert.Throws<InvalidDataException>(() => ChangelogReader.Read(changelog, "0.1.0"));
    }

    [Fact]
    public void Read_YalnizAltBaslikVeParagrafVarsaMaddeYoksa_IhlalVerir()
    {
        var changelog = ChangelogFixtures.With("## [0.1.0] - 2026-10-05", "### Eklendi\n\nBir paragraf ama madde değil.");

        Assert.Throws<InvalidDataException>(() => ChangelogReader.Read(changelog, "0.1.0"));
    }

    [Theory]
    [InlineData("## [0.1.0]")]
    [InlineData("## [0.1.0] - 2026-13-45")]
    [InlineData("## [0.1.0] - 05.10.2026")]
    [InlineData("## [0.1.0] 2026-10-05")]
    public void Read_TarihYoksaYaDaOkunamiyorsa_IhlalVerir(string heading)
    {
        var changelog = ChangelogFixtures.With(heading, "- Bir madde.");

        Assert.Throws<InvalidDataException>(() => ChangelogReader.Read(changelog, "0.1.0"));
    }

    [Fact]
    public void Read_AyniSurumIkiKezGeciyorsa_IhlalVerir()
    {
        var changelog = ChangelogFixtures.Valid + "\n\n## [0.1.0] - 2026-10-06\n\n- Kopya bölüm.\n";

        Assert.Throws<InvalidDataException>(() => ChangelogReader.Read(changelog, "0.1.0"));
    }

    [Fact]
    public void Read_ChangelogBossa_IhlalVerir()
    {
        Assert.Throws<InvalidDataException>(() => ChangelogReader.Read(string.Empty, "0.1.0"));
    }
}
