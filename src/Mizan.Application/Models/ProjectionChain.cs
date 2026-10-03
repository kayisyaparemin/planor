using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Açık dönemden sonraki 12 dönemin kurulduğu plan (S74-1): açılış ana sayfanın dönem sonu, ilk dönem açık dönemin
/// bittiği gün. 12 Dönem ile simülatör aynı zincirden hesapladığı için iki ekran aynı dönem için iki ayrı rakam
/// söylemez. Açık dönemin kendi planı da taşınır çünkü simülatörde bugün ile açık dönem sonu arasına düşen deneme
/// zincirde görünmez; etkisi bu planla hesaplanıp zincirin açılışına eklenir (S76-2).
/// </summary>
public sealed record ProjectionChain
{
    /// <summary>Çapası açık dönemin bitişi, açılışı ana sayfanın dönem sonu olan zincir planı.</summary>
    public required FinancialPlan Plan { get; init; }

    /// <summary>Açık dönemi içeren, çapası değiştirilmemiş plan.</summary>
    public required FinancialPlan OpenPeriodPlan { get; init; }

    /// <summary>Hesabın günü.</summary>
    public required DateOnly Today { get; init; }

    /// <summary>Açık dönemin ilk günü.</summary>
    public required DateOnly OpenPeriodStart { get; init; }

    /// <summary>Zincirin ilk döneminin ilk günü: açık dönemin bittiği gün.</summary>
    public required DateOnly FirstPeriodStart { get; init; }

    /// <summary>Ana sayfadaki dönem sonunun faiz öncesi hâli (dönem sonu + KMH faizi).</summary>
    public required decimal OpenPeriodEndingBeforeDeficitInterest { get; init; }
}
