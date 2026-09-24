using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının düzenli gelir akışlarını, gelir tutar geçmişlerini ve tek seferlik arızi gelirlerini
/// tek bir transfer paketi olarak taşıyan ara veri modelidir.
/// Plan okuyucu servislerin bağımlılık sayısını kontrol altında tutmak için vardır (Kural M3).
/// </summary>
public sealed record IncomePlanBundle(
    IReadOnlyList<RecurringIncome> RecurringIncomes,
    IReadOnlyList<IncomeAmountHistory> IncomeHistories,
    IReadOnlyList<AdHocIncome> AdHocIncomes);
