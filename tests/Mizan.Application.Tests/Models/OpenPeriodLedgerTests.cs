using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Tests.Models;

public sealed class OpenPeriodLedgerTests
{
    [Fact]
    public void CurrentPaymentLines_RevizyonYoksa_DondurulanPlaninSatirlaridir()
    {
        // Hazırla
        var kira = new PeriodPlanPaymentLine { Name = "Kira", PlannedAmount = 15_000m };
        var defter = new OpenPeriodLedger(new PeriodPlanSnapshot { PaymentLines = [kira] }, [], [], [], []);

        // Uygula
        var satirlar = defter.CurrentPaymentLines;

        // Doğrula
        Assert.Equal([kira.Id], satirlar.Select(x => x.Id));
    }

    [Fact]
    public void CurrentPaymentLines_RevizyonVarsa_SonRevizyonunSatirlaridir()
    {
        // Hazırla — revizyonlar defterde en eskiden en yeniye sıralı durur
        var dondurulan = new PeriodPlanPaymentLine { Name = "Kira", PlannedAmount = 15_000m };
        var birinci = new PeriodPlanPaymentLine { Name = "Kira", PlannedAmount = 16_000m };
        var ikinci = new PeriodPlanPaymentLine { Name = "Kira", PlannedAmount = 17_000m };
        var defter = new OpenPeriodLedger(
            new PeriodPlanSnapshot { PaymentLines = [dondurulan] },
            [
                new PeriodPlanRevision { RevisionNumber = 1, PaymentLines = [birinci] },
                new PeriodPlanRevision { RevisionNumber = 2, PaymentLines = [ikinci] }
            ],
            [],
            [],
            []);

        // Uygula
        var satirlar = defter.CurrentPaymentLines;

        // Doğrula
        Assert.Equal([ikinci.Id], satirlar.Select(x => x.Id));
    }

    [Fact]
    public void LatestRevision_RevizyonYoksa_NullDondurur()
    {
        // Hazırla
        var defter = new OpenPeriodLedger(new PeriodPlanSnapshot(), [], [], [], []);

        // Uygula
        var sonRevizyon = defter.LatestRevision;

        // Doğrula
        Assert.Null(sonRevizyon);
    }

    [Fact]
    public void CurrentIncomeLines_RevizyonYoksa_DondurulanPlaninGelirSatirlaridir()
    {
        // Hazırla
        var gelir = new PeriodPlanIncomeLine { Name = "Gelir", PlannedAmount = 40_000m };
        var defter = new OpenPeriodLedger(new PeriodPlanSnapshot { IncomeLines = [gelir] }, [], [], [], []);

        // Uygula
        var satirlar = defter.CurrentIncomeLines;

        // Doğrula
        Assert.Equal([gelir.Id], satirlar.Select(x => x.Id));
    }

    [Fact]
    public void CurrentIncomeLines_RevizyonVarsa_SonRevizyonunGelirSatirlaridir()
    {
        // Hazırla — revizyon gelirin yatış gününü değiştirmiş (I29)
        var dondurulan = new PeriodPlanIncomeLine { Name = "Gelir", PlannedDate = new DateOnly(2026, 9, 15), PlannedAmount = 40_000m };
        var revize = dondurulan with { Id = Guid.NewGuid(), PlannedDate = new DateOnly(2026, 9, 10) };
        var revizyon = new PeriodPlanRevision { RevisionNumber = 1, IncomeLines = [revize] };
        var defter = new OpenPeriodLedger(new PeriodPlanSnapshot { IncomeLines = [dondurulan] }, [revizyon], [], [], []);

        // Uygula
        var satirlar = defter.CurrentIncomeLines;

        // Doğrula
        Assert.Equal([revize.Id], satirlar.Select(x => x.Id));
        Assert.Same(revizyon, defter.LatestRevision);
    }

    [Fact]
    public void LatestObservation_EnGecTarihliGozlemiDoner_GirisSirasinaBakmaz()
    {
        // Hazırla — 20'sindeki gözlem önce, geriye tarihli 10'undaki sonra girilmiş
        var yirmisi = new PeriodObservation { ObservedOn = new DateOnly(2026, 9, 20), ObservedBalance = 5_000m };
        var onu = new PeriodObservation { ObservedOn = new DateOnly(2026, 9, 10), ObservedBalance = 9_000m };
        var defter = new OpenPeriodLedger(new PeriodPlanSnapshot(), [], [yirmisi, onu], [], []);

        // Uygula
        var son = defter.LatestObservation;

        // Doğrula
        Assert.Equal(yirmisi, son);
    }

    [Fact]
    public void LatestObservation_GozlemYoksa_NullDoner()
    {
        var defter = new OpenPeriodLedger(new PeriodPlanSnapshot(), [], [], [], []);

        Assert.Null(defter.LatestObservation);
    }
}
