namespace Mizan.Domain.Models;

/// <summary>
/// Kredi veya kredi kartı haricindeki vadeli borç, senet veya dönemsel ödeme planını temsil eden sözleşme.
/// Nakit akış projeksiyonunda takvimli zorunlu nakit çıkışlarının modellenmesi ve takibi için vardır.
/// </summary>
public sealed record TemporaryPaymentPlan
{
    /// <summary>Ödeme planının benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Ödeme planının kullanıcı tarafından tanımlanan adı (örn. Eminevim, Senetli Mobilya, Okul Taksiti).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Ödeme planının türü (Geçici, Taksitli, Periyodik veya Diğer).</summary>
    public PaymentPlanKind Kind { get; init; } = PaymentPlanKind.Temporary;

    /// <summary>Varsa borcun faiz/vade farkı hariç orijinal anapara tutarı.</summary>
    public decimal? OriginalAmount { get; init; }

    /// <summary>Varsa masraf ve vade farkları dahil toplam geri ödeme tutarı.</summary>
    public decimal? TotalRepaymentAmount { get; init; }

    /// <summary>Plandaki takvimli taksit ve ödeme kalemleri.</summary>
    public IReadOnlyList<TemporaryPaymentInstallment> Installments { get; init; } = [];

    /// <summary>Plandaki tüm taksitlerin nominal toplam tutarı.</summary>
    public decimal TotalInstallmentAmount => Installments.Sum(x => x.Amount);

    /// <summary>Planda henüz ödenmemiş (IsPaid == false) taksitlerin toplam tutarı (kalan borç).</summary>
    public decimal RemainingAmount => Installments.Where(x => !x.IsPaid).Sum(x => x.Amount);

    /// <summary>Planda henüz ödenmemiş kalan taksit adedi.</summary>
    public int RemainingInstallmentCount => Installments.Count(x => !x.IsPaid);

    /// <summary>En az bir taksiti bulunan ve tüm taksitleri ödenmiş olan planın tamamlanma durumu.</summary>
    public bool IsCompleted => Installments.Count > 0 && Installments.All(x => x.IsPaid);

    /// <summary>Ödenmemiş taksitler arasından vadesi en yakın olan bir sonraki taksit.</summary>
    public TemporaryPaymentInstallment? NextInstallment =>
        Installments
            .Where(x => !x.IsPaid)
            .OrderBy(x => x.DueDate)
            .FirstOrDefault();

    /// <summary>
    /// Taksitleri vade tarihine göre kronolojik sıraya dizer ve her taksitin PlanId değerini planın Id'siyle eşitler.
    /// </summary>
    public TemporaryPaymentPlan Normalize() => this with
    {
        Installments = Installments
            .OrderBy(x => x.DueDate)
            .Select(x => x.PlanId == Id ? x : x with { PlanId = Id })
            .ToArray()
    };
}
