using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Düzenli gelirin tutar değişikliklerine dair kuralların tek yeri: hangi tutar ekranda görünür, hangisi
/// silinebilir, yeni tutar ne zaman kabul edilir, bir kayıt ne zaman reddedilir. Gelir formu ile gelir
/// servisi aynı kuralları uygular; iki ayrı kopya olsaydı biri gevşediğinde ekranın kabul ettiğini servis
/// reddeder ya da tersi olurdu (S67-5, S67 V6d2 notları).
/// </summary>
public static class IncomeAmountRules
{
    /// <summary>Tutar sıfır ya da eksi olduğunda gösterilen mesaj.</summary>
    public const string AmountMustBePositiveMessage = "Gelir tutarı sıfırdan büyük olmalıdır.";

    /// <summary>Yeni tutarın geçerlilik tarihi geçmişte olduğunda gösterilen mesaj.</summary>
    public const string PastDateMessage = "Geçerlilik tarihi bugünden önce olamaz.";

    /// <summary>Aynı gelirde aynı tarihte ikinci tutar girildiğinde gösterilen mesaj.</summary>
    public const string DuplicateDateMessage = "Bu tarihte zaten bir tutar var.";

    /// <summary>Yürürlüğe girmiş bir tutar silinmek istendiğinde gösterilen mesaj.</summary>
    public const string EffectiveRemovalMessage = "Yürürlüğe girmiş tutar silinmez.";

    /// <summary>Silinmek istenen kimlik gelirin tutarları arasında olmadığında gösterilen mesaj.</summary>
    public const string UnknownAmountMessage = "Silinecek tutar bu gelire ait değil.";

    /// <summary>Silme gelirin son tutarını götürdüğünde gösterilen mesaj.</summary>
    public const string LastAmountMessage = "Gelirin en az bir tutarı kalmalı.";

    /// <summary>
    /// Bugün yürürlükteki son tutarı ve ileri tarihli değişiklikleri tarihe göre döner. Yürürlükten kalkmış
    /// tutarlar veride kalır ama gösterilmez (S67-5); hiçbiri yürürlükte değilse yalnız ileri tarihliler.
    /// </summary>
    public static IReadOnlyList<IncomeAmountHistory> Upcoming(IEnumerable<IncomeAmountHistory> amounts, DateOnly today)
    {
        var ordered = amounts.OrderBy(x => x.EffectiveDate).ToList();
        var current = ordered.LastOrDefault(x => x.EffectiveDate <= today);
        return ordered.Where(x => x == current || x.EffectiveDate > today).ToArray();
    }

    /// <summary>Tutar bugün ya da sonra yürürlüğe giriyorsa silinebilir; yürürlüğe girmiş tutar tarihçedir (kural 05).</summary>
    public static bool IsRemovable(IncomeAmountHistory amount, DateOnly today) => amount.EffectiveDate >= today;

    /// <summary>
    /// Yeni tutarı gelirin o anki tutarlarına göre denetler: tutar sıfırdan büyük, tarih en erken bugün, aynı
    /// tarihte başka tutar yok. Kabul edilecekse null, edilmeyecekse kullanıcıya gösterilecek mesaj.
    /// </summary>
    public static string? CheckAddition(
        IEnumerable<IncomeAmountHistory> current, IncomeAmountHistory candidate, DateOnly today) =>
        candidate.Amount <= 0m ? AmountMustBePositiveMessage
        : candidate.EffectiveDate < today ? PastDateMessage
        : current.Any(x => x.EffectiveDate == candidate.EffectiveDate) ? DuplicateDateMessage
        : null;

    /// <summary>
    /// Bir kaydı (eklenen tutarlar, silinen kimlikler) gelirin kayıtlı tutarlarına göre denetler. Önce silme
    /// uygulanır, sonra eklenenler sırayla <see cref="CheckAddition"/>'dan geçer. Silme son tutarı götüremez;
    /// hiç silme yoksa tutarı olmayan eski bir gelirin adı da düzenlenebilir.
    /// </summary>
    public static string? CheckChange(
        IReadOnlyCollection<IncomeAmountHistory> existing, IReadOnlyList<IncomeAmountHistory> added,
        IReadOnlyCollection<Guid> removedIds, DateOnly today)
    {
        if (removedIds.Select(id => CheckRemoval(existing, id, today)).FirstOrDefault(x => x is not null) is { } removalError)
        {
            return removalError;
        }

        var remaining = existing.Where(x => !removedIds.Contains(x.Id)).ToList();
        foreach (var amount in added)
        {
            if (CheckAddition(remaining, amount, today) is { } error)
            {
                return error;
            }

            remaining.Add(amount);
        }

        return removedIds.Count > 0 && remaining.Count == 0 ? LastAmountMessage : null;
    }

    private static string? CheckRemoval(IEnumerable<IncomeAmountHistory> existing, Guid id, DateOnly today) =>
        existing.FirstOrDefault(x => x.Id == id) switch
        {
            null => UnknownAmountMessage,
            var removed when !IsRemovable(removed, today) => EffectiveRemovalMessage,
            _ => null
        };
}
