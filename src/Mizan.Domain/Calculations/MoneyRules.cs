namespace Mizan.Domain.Calculations;

/// <summary>
/// Finansal hesaplamalarda kuruş sapmalarını ve yuvarlama hatalarını (drift) önleyen para kuralları.
/// Mizan sisteminde para her zaman 2 ondalık basamaklı ve MidpointRounding.AwayFromZero ile yuvarlanır.
/// Taksit ve eşit bölüştürmelerde kuruş artığı son parçaya eklenerek para kuruşu kuruşuna korunur.
/// </summary>
public static class MoneyRules
{
    /// <summary>
    /// Verilen tutarı kuruş hassasiyetine (2 hane) sıfırdan uzağa (AwayFromZero) yuvarlar.
    /// Ara hesaplamalar tam hassasiyette taşındıktan sonra nihai parasal değerler bu metotla yuvarlanır.
    /// </summary>
    /// <param name="amount">Yuvarlanacak ham parasal tutar.</param>
    /// <returns>Kuruş hassasiyetinde yuvarlanmış tutar.</returns>
    public static decimal Round(decimal amount) =>
        decimal.Round(amount, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Belirtilen toplam tutarı verilen parça sayısına kuruş korunumlu olarak böler.
    /// Kuruş artığı (örneğin 100 TL / 3 taksit durumundaki 1 kuruş) yalnız son parçaya eklenir.
    /// Parçaların toplamı her zaman kuruşu kuruşuna toplam tutara eşittir.
    /// </summary>
    /// <param name="total">Bölüştürülecek toplam tutar.</param>
    /// <param name="count">Bölünecek parça sayısı (1-120).</param>
    /// <returns>Kuruş artığı son parçaya eklenmiş tutar dizisi.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Tutar sıfır/negatifse veya parça sayısı sınır dışındaysa fırlatılır.</exception>
    public static IReadOnlyList<decimal> Distribute(decimal total, int count)
    {
        if (total <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(total), "Toplam tutar sıfırdan büyük olmalıdır.");
        }

        if (count is < 1 or > 120)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Parça sayısı 1 ile 120 arasında olmalıdır.");
        }

        var regular = Round(total / count);
        var paidBeforeLast = regular * (count - 1);
        var last = total - paidBeforeLast;

        var result = new decimal[count];
        for (var i = 0; i < count - 1; i++)
        {
            result[i] = regular;
        }

        result[count - 1] = last;
        return result;
    }
}
