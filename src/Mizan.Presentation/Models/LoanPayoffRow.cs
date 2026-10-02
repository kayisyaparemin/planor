using Mizan.Application.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// "Erken kapama" kartının bir satırı: bir kredi için önerinin sonucu ve önerildiyse gün, tutar, net kazanç (EK-V8 S5).
/// Durum metni App'teki çeviriciyle kurulur; satır yalnız ham değeri taşır (kural: ham veri). Satıra dokunmak
/// krediyi açar, kapama orada planlanır (S74-7).
/// </summary>
public sealed record LoanPayoffRow
{
    /// <summary>Satırın açacağı kredinin kimliği.</summary>
    public required Guid LoanId { get; init; }

    /// <summary>Kredinin banka ve adıyla gösterilen ismi.</summary>
    public required string LoanName { get; init; }

    /// <summary>Önerinin sonucu; satırın durum metnini belirler.</summary>
    public required LoanPayoffAdviceStatus Status { get; init; }

    /// <summary>Önerilen kapatma günü; kapama planlıysa planlanan gün.</summary>
    public DateOnly? Date { get; init; }

    /// <summary>Önerilen gün kapatmak için ödenecek tutar.</summary>
    public decimal? PayoffAmount { get; init; }

    /// <summary>Önerilen gün kapatmanın net kazancı.</summary>
    public decimal? NetGain { get; init; }

    /// <summary>Kapatmak öneriliyor mu; net kazanç yalnız o zaman görünür.</summary>
    public bool IsRecommended => Status == LoanPayoffAdviceStatus.Recommended;
}
