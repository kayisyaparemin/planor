using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Services;

/// <summary>
/// Senaryo: 30 günlük dönem, açılış 30.000, kira 5'inde 15.000, gelir 15'inde 40.000, yaşam havuzu 20.000.
/// 16'sında gözlem yapılırsa 15 gün geçmiştir (0,5); gelir bakiyenin içindedir, kira ödenmiştir; bu yüzden
/// harcanan = 30.000 + 40.000 − 15.000 − bakiye (S70).
/// </summary>
public sealed class SpendingPaceTests
{
    private static readonly DateOnly DonemBasi = new(2026, 9, 1);
    private static readonly DateOnly DonemSonu = new(2026, 10, 1);

    private static readonly PeriodPlanPaymentLine Kira = new()
    {
        SourceEntityId = Guid.NewGuid(),
        SourceType = PlanPaymentSourceType.OtherScheduledPayment,
        Name = "Kira",
        PlannedDate = Gun(5),
        PlannedAmount = 15_000m
    };

    private static readonly PeriodPlanIncomeLine Gelir = new() { Name = "Gelir", PlannedDate = Gun(15), PlannedAmount = 40_000m };

    [Fact]
    public void Calculate_GozlemYoksa_TempoYoktur()
    {
        // Hazırla
        var defter = Defter(PlanKur(yasamHavuzu: 20_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(16));

        // Doğrula
        Assert.Null(gidisat.Pace);
    }

    [Theory]
    [InlineData(45_000, 0.5, 0)]
    [InlineData(40_000, 0.75, 25)]
    [InlineData(50_000, 0.25, -25)]
    public void Calculate_GozlemVarsa_HarcananOranGecenSureyleKiyaslanir(int bakiye, double beklenenHarcanan, int beklenenFark)
    {
        // Hazırla
        var defter = Defter(PlanKur(yasamHavuzu: 20_000m), Gozlem(Gun(16), bakiye));

        // Uygula
        var pace = Hesapla(defter, Gun(16)).Pace;

        // Doğrula — pozitif fark harcamanın önde olduğu demektir
        Assert.NotNull(pace);
        Assert.Equal((decimal)beklenenHarcanan, pace.SpentRatio);
        Assert.Equal(0.5m, pace.ElapsedRatio);
        Assert.Equal(beklenenFark, pace.GapPoints);
    }

    [Fact]
    public void Calculate_BugunGozlemGunundenSonraysa_GecenSureGozlemGunuyleHesaplanir()
    {
        // Hazırla — bakiye 16'sında girildi, bugün 28'i: bugüne göre süre 27/30 olurdu ve harcama "geride" görünürdü
        var defter = Defter(PlanKur(yasamHavuzu: 20_000m), Gozlem(Gun(16), 40_000m));

        // Uygula
        var pace = Hesapla(defter, Gun(28)).Pace;

        // Doğrula
        Assert.NotNull(pace);
        Assert.Equal(Gun(16), pace.ObservedOn);
        Assert.Equal(0.5m, pace.ElapsedRatio);
        Assert.Equal(25m, pace.GapPoints);
    }

    [Fact]
    public void Calculate_HavuzAsildiysa_HarcananOranBireKirpilmaz()
    {
        // Hazırla — harcanan 30.000 + 40.000 − 15.000 − 20.000 = 35.000; havuz 20.000
        var defter = Defter(PlanKur(yasamHavuzu: 20_000m), Gozlem(Gun(16), 20_000m));

        // Uygula
        var pace = Hesapla(defter, Gun(16)).Pace;

        // Doğrula
        Assert.NotNull(pace);
        Assert.Equal(1.75m, pace.SpentRatio);
        Assert.Equal(125m, pace.GapPoints);
    }

    [Fact]
    public void Calculate_HavuzSifirsa_TempoYoktur()
    {
        // Hazırla — oran sıfıra bölünerek tanımsız kalır; "yüzde 0 harcandı, geride" demek yanıltıcı olur
        var defter = Defter(PlanKur(yasamHavuzu: 0m), Gozlem(Gun(16), 45_000m));

        // Uygula
        var gidisat = Hesapla(defter, Gun(16));

        // Doğrula
        Assert.Null(gidisat.Pace);
    }

    [Fact]
    public void Calculate_GozlemDonemBasindaysa_GecenSureSifirdir()
    {
        // Hazırla — dönem başı gözlemi açılış bakiyesidir; hiçbir şey harcanmadı ve süre geçmedi
        var defter = Defter(PlanKur(yasamHavuzu: 20_000m), Gozlem(DonemBasi, 30_000m));

        // Uygula
        var pace = Hesapla(defter, DonemBasi).Pace;

        // Doğrula
        Assert.NotNull(pace);
        Assert.Equal(0m, pace.SpentRatio);
        Assert.Equal(0m, pace.ElapsedRatio);
        Assert.Equal(0m, pace.GapPoints);
    }

    private static PeriodProgress Hesapla(OpenPeriodLedger defter, DateOnly bugun) =>
        PeriodProgressCalculator.Calculate(defter, new Dictionary<Guid, decimal>(), 0.05m, bugun);

    private static OpenPeriodLedger Defter(PeriodPlanSnapshot plan, PeriodObservation? gozlem = null) =>
        new(plan, [], gozlem is null ? [] : [gozlem], [], []);

    private static PeriodPlanSnapshot PlanKur(decimal yasamHavuzu) => new()
    {
        PeriodStart = DonemBasi,
        PeriodEnd = DonemSonu,
        SettlementAvailableFrom = DonemSonu,
        OpeningBalance = 30_000m,
        PlannedIncome = 40_000m,
        PlannedMandatoryPayments = 15_000m,
        PlannedVariableExpenseAllowance = yasamHavuzu,
        PlannedDeficitInterest = 0m,
        PlannedEndingBalance = 55_000m - yasamHavuzu,
        PaymentLines = [Kira],
        IncomeLines = [Gelir]
    };

    private static PeriodObservation Gozlem(DateOnly gun, decimal bakiye) => new()
    {
        ObservedOn = gun,
        ObservedBalance = bakiye,
        RecordedAtUtc = new DateTimeOffset(gun.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero)
    };

    private static DateOnly Gun(int gun) => new(2026, 9, gun);
}
