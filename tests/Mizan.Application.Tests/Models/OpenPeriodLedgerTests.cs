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
        var defter = new OpenPeriodLedger(new PeriodPlanSnapshot { PaymentLines = [kira] }, [], null, []);

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
            null,
            []);

        // Uygula
        var satirlar = defter.CurrentPaymentLines;

        // Doğrula
        Assert.Equal([ikinci.Id], satirlar.Select(x => x.Id));
    }
}
