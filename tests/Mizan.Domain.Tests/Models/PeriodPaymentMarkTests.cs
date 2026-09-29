using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodPaymentMarkTests
{
    [Fact]
    public void Yeni_VarsayilanDegerler_BeklenenSekildedir()
    {
        // Uygula
        var isaret = new PeriodPaymentMark();

        // Doğrula
        Assert.NotEqual(Guid.Empty, isaret.Id);
        Assert.Equal(Guid.Empty, isaret.PeriodPlanSnapshotId);
        Assert.Equal(Guid.Empty, isaret.PeriodPlanPaymentLineId);
        Assert.Equal(0m, isaret.ActualAmount);
        Assert.Null(isaret.ActualPaymentDate);
        Assert.Equal(string.Empty, isaret.Note);
    }

    [Theory]
    [InlineData(ActualPaymentStatus.Paid, true)]
    [InlineData(ActualPaymentStatus.DifferentAmount, true)]
    [InlineData(ActualPaymentStatus.Unpaid, false)]
    public void IsSettled_DurumaGore_DogruSonucUretir(ActualPaymentStatus durum, bool beklenen)
    {
        // Hazırla
        var isaret = new PeriodPaymentMark { Status = durum };

        // Doğrula
        Assert.Equal(beklenen, isaret.IsSettled);
    }

    [Theory]
    [InlineData(12_500, 10_000, 2_500)]
    [InlineData(12_500, 15_000, -2_500)]
    public void CalculateVariance_PlanlananIleFiiliFarkiniHesaplar(int fiili, int planlanan, int beklenenFark)
    {
        // Hazırla
        var isaret = new PeriodPaymentMark { ActualAmount = fiili };

        // Uygula
        var fark = isaret.CalculateVariance(planlanan);

        // Doğrula
        Assert.Equal(beklenenFark, fark);
    }

    [Fact]
    public void CalculateVariance_PlanlananTutarYoksa_SifirKabulEder()
    {
        // Hazırla
        var isaret = new PeriodPaymentMark { ActualAmount = 12_500m };

        // Uygula
        var fark = isaret.CalculateVariance(null);

        // Doğrula
        Assert.Equal(12_500m, fark);
    }
}
