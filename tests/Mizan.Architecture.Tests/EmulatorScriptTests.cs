namespace Mizan.Architecture.Tests;

/// <summary>
/// Emülatör regresyon betiğinin S81'deki sözleri tuttuğunu sabitler (K3).
/// Kaynak dosya okuduğu için burada durur (K7).
/// </summary>
public sealed class EmulatorScriptTests
{
    private const string GoodScript = """
        $baslangic = Get-Date
        & "$PSScriptRoot\emulatorde-ac.ps1" -Avd $Avd -YedekKlasoru $YedekKlasoru
        if ($LASTEXITCODE -ne 0) { exit 1 }
        $maestroKodu = 1
        try {
            & maestro --device $serial test $Akis
            $maestroKodu = $LASTEXITCODE
        }
        finally {
            & adb -s $serial shell run-as com.mizan.app rm -rf files/profiles
            & "$PSScriptRoot\emulatorde-ac.ps1" -GeriYukle $yedek
        }
        if ($maestroKodu -eq 0) { Write-Host "Regresyon geçti." }
        exit $maestroKodu
        """;

    [Fact]
    public void Betik_S81SozleriniTasir()
    {
        Assert.Empty(EmulatorScriptRules.CheckScript());
    }

    [Fact]
    public void Kural_DogruIcerikteIhlalBulmaz()
    {
        Assert.Empty(EmulatorScriptRules.CheckScriptContent(GoodScript));
    }

    [Theory]
    [InlineData("--device $serial test $Akis", "--device $serial test")]
    [InlineData("-GeriYukle $yedek", "-Avd $yedek")]
    [InlineData("rm -rf files/profiles", "ls files/profiles")]
    [InlineData("finally", "catch")]
    [InlineData("exit $maestroKodu", "exit 0")]
    [InlineData("$maestroKodu = $LASTEXITCODE", "$maestroKodu = 0")]
    [InlineData("emulatorde-ac.ps1\" -Avd", "baska.ps1\" -Avd")]
    public void Kural_EksikKorumayiYakalar(string remove, string with)
    {
        Assert.NotEmpty(EmulatorScriptRules.CheckScriptContent(GoodScript.Replace(remove, with)));
    }

    [Theory]
    [InlineData("& adb shell input tap 75 136")]
    [InlineData("& adb shell input swipe 1 2 3 4")]
    [InlineData("& adb shell uiautomator dump /sdcard/w.xml")]
    [InlineData("function Record-Test { }")]
    public void Kural_KoordinatVeEkranDokumuYasak(string line)
    {
        Assert.NotEmpty(EmulatorScriptRules.CheckScriptContent(GoodScript + "\n" + line));
    }

    [Fact]
    public void Kural_GeriYuklemeAkisinOncesindeyseYakalar()
    {
        var restoreBeforeFlow = """
            & "$PSScriptRoot\emulatorde-ac.ps1" -GeriYukle $yedek
            """;
        var bad = restoreBeforeFlow + "\n" + GoodScript.Replace("-GeriYukle $yedek", "-Avd $yedek");
        Assert.NotEmpty(EmulatorScriptRules.CheckScriptContent(bad.Replace("finally", "catch")));
    }
}
