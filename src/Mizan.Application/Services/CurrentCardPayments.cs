using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Kartın bugünkü hâliyle açık dönemde vadesi gelen ödemesi. Dondurulan plan kartın o günkü tahminini saklar;
/// aradan geçen ekstre girişleri, harcamalar ve ödeme kararları onu değiştirmez (I23). Ana sayfanın "şu an"ı ile
/// hatırlatıcı aynı tutarı söylemezse kullanıcı hangisine güveneceğini bilemez; iki taraf da tutarı buradan alır (M8).
/// </summary>
public static class CurrentCardPayments
{
    // Kartın son bilinen ekstresinden açık dönemin vadesine uzanmaya yeten ekstre sayısı.
    private const int ProjectedStatementCount = 6;

    /// <summary>
    /// Kart kimliğine göre bu dönemde vadesi gelen ödemeyi döner. Pasif ya da bakiyesi hiç girilmemiş kart yer
    /// almaz; o kartın satırı plandaki tutarıyla kalır (<see cref="ProjectedPaymentAmount"/>).
    /// </summary>
    /// <param name="cards">Kullanıcının kayıtlı kartları.</param>
    /// <param name="period">Açık dönem; vade plan satırıyla aynı yarı açık pencereden seçilir (S30).</param>
    /// <param name="carryInterestRate">Devreden kart borcunun dönemlik faiz oranı.</param>
    /// <param name="statementCalculator">Kartın ekstrelerini ileriye yürüten hesaplayıcı.</param>
    public static IReadOnlyDictionary<Guid, decimal> Of(
        IReadOnlyList<CreditCard> cards,
        CashFlowPeriod period,
        decimal carryInterestRate,
        CreditCardStatementCalculator statementCalculator) =>
        cards
            .Where(card => card.IsActive && card.BalanceAsOfDate != default)
            .ToDictionary(
                card => card.Id,
                card => statementCalculator
                    .Project(card, ProjectedStatementCount, useProjectionFallback: true, carryInterestRate: carryInterestRate)
                    .Where(x => period.Contains(x.PaymentDueDate))
                    .Sum(x => x.Payment ?? 0m));
}
