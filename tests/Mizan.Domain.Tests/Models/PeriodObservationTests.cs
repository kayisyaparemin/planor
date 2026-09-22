using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Models;

public sealed class PeriodObservationTests
{
    [Fact]
    public void PeriodObservationPayment_VarsayilanDegerler_BeklenenSekildedir()
    {
        // Act
        var payment = new PeriodObservationPayment();

        // Assert
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(Guid.Empty, payment.PeriodObservationId);
        Assert.Equal(Guid.Empty, payment.PeriodPlanPaymentLineId);
        Assert.Equal(ActualPaymentStatus.Paid, payment.Status);
        Assert.Equal(0m, payment.ActualAmount);
        Assert.Null(payment.ActualPaymentDate);
        Assert.Equal(string.Empty, payment.Note);
        Assert.True(payment.IsSettled);
    }

    [Theory]
    [InlineData(ActualPaymentStatus.Paid, true)]
    [InlineData(ActualPaymentStatus.DifferentAmount, true)]
    [InlineData(ActualPaymentStatus.Unpaid, false)]
    public void PeriodObservationPayment_IsSettled_DurumaGoreDogruSonucUretir(
        ActualPaymentStatus status,
        bool expectedIsSettled)
    {
        // Arrange
        var payment = new PeriodObservationPayment
        {
            Status = status
        };

        // Assert
        Assert.Equal(expectedIsSettled, payment.IsSettled);
    }

    [Fact]
    public void PeriodObservationPayment_CalculateVariance_PlanlananIleFiiliFarkiniDogruHesaplar()
    {
        // Arrange
        var payment = new PeriodObservationPayment
        {
            ActualAmount = 12_500m
        };

        // Act & Assert
        // Planlanan 10.000, Fiili 12.500 -> Fark +2.500
        Assert.Equal(2_500m, payment.CalculateVariance(10_000m));

        // Planlanan 15.000, Fiili 12.500 -> Fark -2.500
        Assert.Equal(-2_500m, payment.CalculateVariance(15_000m));

        // Planlanan null ise 0 kabul edilir -> Fark +12.500
        Assert.Equal(12_500m, payment.CalculateVariance(null));
    }

    [Fact]
    public void PeriodObservation_VarsayilanDegerler_BeklenenSekildedir()
    {
        // Act
        var observation = new PeriodObservation();

        // Assert
        Assert.NotEqual(Guid.Empty, observation.Id);
        Assert.Equal(Guid.Empty, observation.PeriodPlanSnapshotId);
        Assert.Equal(default, observation.ObservedOn);
        Assert.Null(observation.ObservedBalance);
        Assert.False(observation.HasObservedBalance);
        Assert.Equal(0m, observation.ObservedLivingSpend);
        Assert.Equal(string.Empty, observation.Note);
        Assert.Empty(observation.Payments);
        Assert.Equal(0m, observation.TotalObservedPayments);
    }

    [Fact]
    public void PeriodObservation_HasObservedBalance_BakiyeGirilipGirilmediginiDogrular()
    {
        // Arrange & Act
        var emptyObservation = new PeriodObservation { ObservedBalance = null };
        var zeroObservation = new PeriodObservation { ObservedBalance = 0m };
        var negativeObservation = new PeriodObservation { ObservedBalance = -107_150m };
        var positiveObservation = new PeriodObservation { ObservedBalance = 25_000m };

        // Assert
        Assert.False(emptyObservation.HasObservedBalance);
        Assert.True(zeroObservation.HasObservedBalance);
        Assert.True(negativeObservation.HasObservedBalance);
        Assert.True(positiveObservation.HasObservedBalance);
    }

    [Fact]
    public void PeriodObservation_TotalObservedPayments_YalnizcaOdenmisleriToplar()
    {
        // Arrange
        var observation = new PeriodObservation
        {
            Payments =
            [
                new PeriodObservationPayment
                {
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = 5_000m
                },
                new PeriodObservationPayment
                {
                    Status = ActualPaymentStatus.DifferentAmount,
                    ActualAmount = 2_500m
                },
                new PeriodObservationPayment
                {
                    Status = ActualPaymentStatus.Unpaid,
                    ActualAmount = 3_000m // Unpaid olduğu için toplama girmemeli
                }
            ]
        };

        // Act
        var total = observation.TotalObservedPayments;

        // Assert
        // 5.000 + 2.500 = 7.500
        Assert.Equal(7_500m, total);
    }

    [Fact]
    public void PeriodObservation_FindPayment_Ve_IsPaymentSettled_DogruCalisir()
    {
        // Arrange
        var lineId1 = Guid.NewGuid();
        var lineId2 = Guid.NewGuid();
        var lineId3 = Guid.NewGuid();
        var olmayanLineId = Guid.NewGuid();

        var observation = new PeriodObservation
        {
            Payments =
            [
                new PeriodObservationPayment
                {
                    PeriodPlanPaymentLineId = lineId1,
                    Status = ActualPaymentStatus.Paid,
                    ActualAmount = 1_000m
                },
                new PeriodObservationPayment
                {
                    PeriodPlanPaymentLineId = lineId2,
                    Status = ActualPaymentStatus.Unpaid,
                    ActualAmount = 0m
                }
            ]
        };

        // Assert - FindPayment
        Assert.NotNull(observation.FindPayment(lineId1));
        Assert.Equal(lineId1, observation.FindPayment(lineId1)!.PeriodPlanPaymentLineId);
        Assert.NotNull(observation.FindPayment(lineId2));
        Assert.Null(observation.FindPayment(olmayanLineId));

        // Assert - IsPaymentSettled
        Assert.True(observation.IsPaymentSettled(lineId1));
        Assert.False(observation.IsPaymentSettled(lineId2)); // Unpaid
        Assert.False(observation.IsPaymentSettled(olmayanLineId)); // Listede yok
        Assert.False(observation.IsPaymentSettled(lineId3)); // Listede yok
    }
}
