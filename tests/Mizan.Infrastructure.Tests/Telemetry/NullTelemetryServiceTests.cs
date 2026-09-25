using Mizan.Infrastructure.Telemetry;

namespace Mizan.Infrastructure.Tests.Telemetry;

public sealed class NullTelemetryServiceTests
{
    [Fact]
    public void TrackEvent_OzellikleriyleCagrilinca_IstisnaFirlatmaz()
    {
        // Hazırla
        var telemetry = new NullTelemetryService();
        var ozellikler = new Dictionary<string, string> { ["ekran"] = "dashboard", ["tutar"] = "85000" };

        // Uygula
        var istisna = Record.Exception(() => telemetry.TrackEvent("period_selected", ozellikler));

        // Doğrula
        Assert.Null(istisna);
    }

    [Fact]
    public void TrackInvariantViolation_AyrintilariylaCagrilinca_IstisnaFirlatmaz()
    {
        // Hazırla
        var telemetry = new NullTelemetryService();
        var ayrintilar = new Dictionary<string, string> { ["planId"] = "plan_123", ["amount"] = "1500" };

        // Uygula
        var istisna = Record.Exception(
            () => telemetry.TrackInvariantViolation("I16", "Dondurulmuş plan değişti", ayrintilar));

        // Doğrula
        Assert.Null(istisna);
    }

    [Fact]
    public void CaptureException_BaglamiylaCagrilinca_IstisnaFirlatmaz()
    {
        // Hazırla
        var telemetry = new NullTelemetryService();
        var yakalanan = new InvalidOperationException("Beklenmeyen veritabanı durumu");
        var baglam = new Dictionary<string, string> { ["ekran"] = "settings" };

        // Uygula
        var istisna = Record.Exception(() => telemetry.CaptureException(yakalanan, baglam));

        // Doğrula
        Assert.Null(istisna);
    }

    [Fact]
    public void AddBreadcrumb_VarsayilanKategoriyle_IstisnaFirlatmaz()
    {
        // Hazırla
        var telemetry = new NullTelemetryService();

        // Uygula
        var istisna = Record.Exception(() => telemetry.AddBreadcrumb("Ana sayfaya dönüldü"));

        // Doğrula
        Assert.Null(istisna);
    }
}
