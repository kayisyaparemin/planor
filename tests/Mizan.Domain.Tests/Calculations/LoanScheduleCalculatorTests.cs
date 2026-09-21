using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

public sealed class LoanScheduleCalculatorTests
{
    private readonly LoanScheduleCalculator _calculator = new();

    [Fact]
    public void GetPaymentDates_IlkTaksitTarihindenBaslarVeVadeGununuKullanir()
    {
        var loan = new Loan
        {
            MonthlyPayment = 2_500m,
            PaymentDay = 15,
            NextPaymentDate = new DateOnly(2026, 9, 20),
            RemainingInstallmentCount = 3
        };

        var dates = _calculator.GetPaymentDates(loan);

        Assert.Equal(3, dates.Count);
        Assert.Equal(new DateOnly(2026, 9, 20), dates[0]);
        Assert.Equal(new DateOnly(2026, 10, 15), dates[1]);
        Assert.Equal(new DateOnly(2026, 11, 15), dates[2]);
    }

    [Fact]
    public void GetPaymentDates_AySonuKisaAydanSonraUzunAyaGecildiginde_VadeGununuGeriKazanir()
    {
        // 31 gün vadesi: 31 Ocak -> 28 Şubat (artık olmayan yıl) -> 31 Mart
        var loan = new Loan
        {
            MonthlyPayment = 1_000m,
            PaymentDay = 31,
            NextPaymentDate = new DateOnly(2027, 1, 31),
            RemainingInstallmentCount = 3
        };

        var dates = _calculator.GetPaymentDates(loan);

        Assert.Equal(
            [
                new DateOnly(2027, 1, 31),
                new DateOnly(2027, 2, 28),
                new DateOnly(2027, 3, 31)
            ],
            dates);
    }

    [Fact]
    public void GetPaymentDates_ArtikYilSubatAyinda_29SubatOlarakUretir()
    {
        // 2028 artık yıldır. 31 Ocak -> 29 Şubat -> 31 Mart
        var loan = new Loan
        {
            MonthlyPayment = 5_000m,
            PaymentDay = 31,
            NextPaymentDate = new DateOnly(2028, 1, 31),
            RemainingInstallmentCount = 3
        };

        var dates = _calculator.GetPaymentDates(loan);

        Assert.Equal(
            [
                new DateOnly(2028, 1, 31),
                new DateOnly(2028, 2, 29),
                new DateOnly(2028, 3, 31)
            ],
            dates);
    }

    [Fact]
    public void GetPaymentDates_KalanTaksitSayisiKadarTarihUretir()
    {
        var loan = new Loan
        {
            MonthlyPayment = 10_000m,
            PaymentDay = 10,
            NextPaymentDate = new DateOnly(2026, 10, 10),
            RemainingInstallmentCount = 12
        };

        var dates = _calculator.GetPaymentDates(loan);

        Assert.Equal(12, dates.Count);
        Assert.Equal(new DateOnly(2026, 10, 10), dates[0]);
        Assert.Equal(new DateOnly(2027, 9, 10), dates[11]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void GetPaymentDates_KalanTaksitSifirVeyaNegatifse_BosDiziDoner(int remainingCount)
    {
        var loan = new Loan
        {
            MonthlyPayment = 1_000m,
            PaymentDay = 5,
            NextPaymentDate = new DateOnly(2026, 9, 5),
            RemainingInstallmentCount = remainingCount
        };

        var dates = _calculator.GetPaymentDates(loan);

        Assert.Empty(dates);
    }

    [Fact]
    public void GetPaymentDates_LoanNullIse_ArgumentNullExceptionFirlatir()
    {
        Assert.Throws<ArgumentNullException>(() => _calculator.GetPaymentDates(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void GetPaymentDates_AylikTaksitSifirVeyaNegatifIse_ArgumentOutOfRangeExceptionFirlatir(decimal monthlyPayment)
    {
        var loan = new Loan
        {
            MonthlyPayment = monthlyPayment,
            PaymentDay = 10,
            NextPaymentDate = new DateOnly(2026, 9, 10),
            RemainingInstallmentCount = 5
        };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.GetPaymentDates(loan));
        Assert.Equal("loan", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(32)]
    public void GetPaymentDates_OdemeGunuGecersizse_ArgumentOutOfRangeExceptionFirlatir(int invalidDay)
    {
        var loan = new Loan
        {
            MonthlyPayment = 1_000m,
            PaymentDay = invalidDay,
            NextPaymentDate = new DateOnly(2026, 9, 10),
            RemainingInstallmentCount = 5
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.GetPaymentDates(loan));
    }

    [Fact]
    public void GetPaymentDates_SonrakiOdemeTarihiTanimsizsa_InvalidOperationExceptionFirlatir()
    {
        var loan = new Loan
        {
            MonthlyPayment = 1_000m,
            PaymentDay = 10,
            NextPaymentDate = default,
            RemainingInstallmentCount = 5
        };

        Assert.Throws<InvalidOperationException>(() => _calculator.GetPaymentDates(loan));
    }
}
