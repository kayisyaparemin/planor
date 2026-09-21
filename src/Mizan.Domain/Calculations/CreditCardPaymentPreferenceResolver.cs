using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kredi kartının geçmiş ekstre ödeme tercihlerini (asgari, tamamı, özel tutar) etkin tarih (effective-date)
/// mantığıyla çözümleyen saf hesaplayıcı.
/// Kullanıcının ekstre bazlı ödeme kararları append-only olarak saklanır; bu sınıf belirli bir hesap kesim
/// tarihinde yürürlükte olan en güncel tercihi belirler, geçmişin kronolojik sıralamasını sunar ve
/// mükerrer karar kayıtlarını ayırt ederek geçmişin gereksiz şişmesini önler.
/// </summary>
public sealed class CreditCardPaymentPreferenceResolver
{
    /// <summary>
    /// Belirtilen ekstre kesim tarihi itibarıyla yürürlükte olan en güncel ödeme tercihini belirler.
    /// Kesim tarihinden önce veya kesim tarihinde yürürlüğe girmiş kayıtlar taranır; en yeni etkin tarihli
    /// (aynı tarihte birden fazla kayıt varsa en son oluşturulan) tercih seçilir. Uygun kayıt yoksa null döner.
    /// </summary>
    /// <param name="statementDate">Ödeme tercihinin sorgulandığı ekstre kesim tarihi.</param>
    /// <param name="history">Kartın tarihsel ödeme tercihi kayıtları.</param>
    /// <returns>Yürürlükteki ödeme tercihi veya henüz hiçbir tercih oluşmamışsa null.</returns>
    public CreditCardPaymentPreference? Resolve(
        DateOnly statementDate,
        IEnumerable<CreditCardPaymentPreference> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        return history
            .Where(x => x.EffectiveFromStatementDate <= statementDate)
            .OrderByDescending(x => x.EffectiveFromStatementDate)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Ödeme tercihi geçmişini eskiden yeniye doğru kronolojik olarak sıralar.
    /// Kullanıcı arayüzü ve dökümler için tekil sıralama kaynağıdır.
    /// </summary>
    /// <param name="history">Sıralanacak ödeme tercihi kayıtları.</param>
    /// <returns>Kronolojik sıralı tercih listesi.</returns>
    public IReadOnlyList<CreditCardPaymentPreference> Ordered(
        IEnumerable<CreditCardPaymentPreference> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        return history
            .OrderBy(x => x.EffectiveFromStatementDate)
            .ThenBy(x => x.CreatedAt)
            .ToArray();
    }

    /// <summary>
    /// Yürürlükteki mevcut ödeme tercihi ile yeni girilen ekstre ödeme planının iş mantığı açısından
    /// aynı kararı temsil edip etmediğini denetler.
    /// Aynı kararın tekrar girilmesi durumunda mükerrer tarihçe kaydı üretilmesini engeller.
    /// </summary>
    /// <param name="preference">Hâlihazırda yürürlükte olan ödeme tercihi (varsa).</param>
    /// <param name="plan">Yeni belirlenen mevcut ekstre ödeme planı (varsa).</param>
    /// <returns>İki karar ödeme modu ve tutar bakımından özdeş ise true; aksi hâlde false.</returns>
    public bool RepresentsSameDecision(
        CreditCardPaymentPreference? preference,
        CurrentStatementPaymentPlan? plan)
    {
        if (preference is null || plan is null)
        {
            return preference is null && plan is null;
        }

        return preference.Mode == plan.Mode &&
               NormalizeAmount(preference.Mode, preference.CustomAmount) ==
               NormalizeAmount(plan.Mode, plan.CustomAmount);
    }

    /// <summary>
    /// Ödeme tercihi geçmişindeki tüm kayıtların iş kurallarına uygunluğunu doğrular.
    /// Geçersiz mod, tanımsız tarih veya sıfır/negatif özel tutar tespit edilirse hata fırlatır.
    /// </summary>
    /// <param name="history">Doğrulanacak ödeme tercihi kayıtları.</param>
    public void Validate(
        IEnumerable<CreditCardPaymentPreference> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        foreach (var preference in history)
        {
            if (!Enum.IsDefined(preference.Mode))
            {
                throw new InvalidOperationException(
                    "Ödeme tercihi geçmişinde geçersiz ödeme şekli var.");
            }

            if (preference.EffectiveFromStatementDate == default)
            {
                throw new InvalidOperationException(
                    "Ödeme tercihi geçmişi için geçerli bir kesim tarihi gereklidir.");
            }

            if (preference.Mode == CurrentStatementPaymentMode.Custom &&
                preference.CustomAmount is null or <= 0m)
            {
                throw new InvalidOperationException(
                    "Özel ödeme tercihi için sıfırdan büyük bir tutar gereklidir.");
            }
        }
    }

    private static decimal? NormalizeAmount(
        CurrentStatementPaymentMode mode,
        decimal? amount) =>
        mode == CurrentStatementPaymentMode.Custom ? amount : null;
}
