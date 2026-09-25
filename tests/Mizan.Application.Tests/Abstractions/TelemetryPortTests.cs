using System.Reflection;
using Mizan.Application.Abstractions;
using Mizan.Application.Tests.Fakes;

namespace Mizan.Application.Tests.Abstractions;

public sealed class TelemetryPortTests
{
    [Fact]
    public void TelemetryService_ArayuzOlmalidir()
    {
        var type = typeof(ITelemetryService);
        Assert.True(type.IsInterface, "ITelemetryService bir interface olmalıdır.");
    }

    [Fact]
    public void TelemetryService_KuralM5_OnMetottanAzOlmali()
    {
        var type = typeof(ITelemetryService);
        var methodCount = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Length;
        Assert.True(methodCount <= 10, $"ITelemetryService M5 kuralını ihlal ediyor: {methodCount} metot var (en fazla 10 olabilir).");
        Assert.Equal(4, methodCount);
    }

    [Fact]
    public void TelemetryService_TrackEvent_OlayiKaydetmelidir()
    {
        var telemetry = new FakeTelemetryService();
        var props = new Dictionary<string, string> { ["source"] = "unit_test", ["action"] = "click" };

        telemetry.TrackEvent("period_selected", props);

        var recorded = Assert.Single(telemetry.Events);
        Assert.Equal("period_selected", recorded.Name);
        Assert.NotNull(recorded.Properties);
        Assert.Equal("unit_test", recorded.Properties["source"]);
        Assert.Equal("click", recorded.Properties["action"]);
    }

    [Fact]
    public void TelemetryService_TrackInvariantViolation_IhlaliKaydetmelidir()
    {
        var telemetry = new FakeTelemetryService();
        var details = new Dictionary<string, string> { ["planId"] = "plan_123", ["amount"] = "1500" };

        telemetry.TrackInvariantViolation("I16", "Plan dondurulduktan sonra mutasyona uğrayamaz", details);

        var recorded = Assert.Single(telemetry.InvariantViolations);
        Assert.Equal("I16", recorded.Code);
        Assert.Equal("Plan dondurulduktan sonra mutasyona uğrayamaz", recorded.Description);
        Assert.NotNull(recorded.Details);
        Assert.Equal("plan_123", recorded.Details["planId"]);
    }

    [Fact]
    public void TelemetryService_CaptureException_IstisnayiKaydetmelidir()
    {
        var telemetry = new FakeTelemetryService();
        var ex = new InvalidOperationException("Beklenmeyen veritabanı durumu");
        var context = new Dictionary<string, string> { ["screen"] = "dashboard" };

        telemetry.CaptureException(ex, context);

        var recorded = Assert.Single(telemetry.Exceptions);
        Assert.Same(ex, recorded.Exception);
        Assert.NotNull(recorded.Context);
        Assert.Equal("dashboard", recorded.Context["screen"]);
    }

    [Fact]
    public void TelemetryService_AddBreadcrumb_EkmekKirintisiniKaydetmelidir()
    {
        var telemetry = new FakeTelemetryService();

        telemetry.AddBreadcrumb("Ana sayfaya dönüldü", "navigation");

        var recorded = Assert.Single(telemetry.Breadcrumbs);
        Assert.Equal("Ana sayfaya dönüldü", recorded.Message);
        Assert.Equal("navigation", recorded.Category);
    }

    [Fact]
    public void TelemetryService_Clear_KayitlariTemizlemelidir()
    {
        var telemetry = new FakeTelemetryService();
        telemetry.TrackEvent("ev1");
        telemetry.TrackInvariantViolation("I1", "desc");
        telemetry.CaptureException(new InvalidOperationException("test"));
        telemetry.AddBreadcrumb("crumb");

        telemetry.Clear();

        Assert.Empty(telemetry.Events);
        Assert.Empty(telemetry.InvariantViolations);
        Assert.Empty(telemetry.Exceptions);
        Assert.Empty(telemetry.Breadcrumbs);
    }
}
