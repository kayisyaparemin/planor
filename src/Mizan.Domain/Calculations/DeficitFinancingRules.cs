namespace Mizan.Domain.Calculations;

/// <summary>
/// Dönem sonu nakit eksiye düştüğünde doğan finansman açığı (KMH) faizinin tek kaynağı.
/// Aynı kural hem 12 dönemlik projeksiyonda (dönem başında dondurulan plan buradan çıkar)
/// hem de açık dönemin gidişatında (kullanıcı bakiyesini girdiğinde dönem sonu nereye gidiyor)
/// işler. Eski projede ikincisi birincisinden elle kopyalanmıştı ("motorla birebir aynı kural");
/// biri değiştiğinde diğeri sessizce yanlış kalacaktı (düğüm T6). Bu yüzden kural burada durur
/// ve iki taraf da onu çağırır.
/// </summary>
public static class DeficitFinancingRules
{
    /// <summary>
    /// Faiz öncesi dönem sonu bakiyesi eksiyse açığın tamamına dönemlik oranı uygular ve kuruşa
    /// yuvarlar; bakiye sıfır veya artıdaysa açık yoktur, faiz de yoktur. Faiz dönem sonundan düşülür
    /// ve eksi bakiye sonraki döneme devrettiği için telafi edilene kadar yeniden faiz doğurur (I17).
    /// </summary>
    /// <param name="endingBalanceBeforeInterest">Dönemin faiz işletilmeden önceki kapanış bakiyesi.</param>
    /// <param name="interestRate">Kullanıcının belirlediği dönemlik finansman açığı faiz oranı (0,05 = %5).</param>
    /// <returns>Dönem sonundan düşülecek, sıfır veya pozitif faiz tutarı.</returns>
    public static decimal CalculateInterest(decimal endingBalanceBeforeInterest, decimal interestRate) =>
        endingBalanceBeforeInterest < 0m
            ? MoneyRules.Round(Math.Abs(endingBalanceBeforeInterest) * interestRate)
            : 0m;
}
