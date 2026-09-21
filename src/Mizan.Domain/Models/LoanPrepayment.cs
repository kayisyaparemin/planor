namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının mevcut bir kredisine yönelik gerçekleştirmeyi planladığı veya gerçekleştirdiği
/// kısmi ara ödeme veya erken kapama taahhüdünü temsil eder.
/// Kredinin orijinal sözleşme yapısını bozmadan, itfa simülatöründe taksit ve anapara
/// yeniden hesaplamalarının (kalan vade veya taksit indirimi) dinamik olarak oynatılmasını sağlar.
/// </summary>
public sealed record LoanPrepayment
{
    /// <summary>Erken ödeme kaydının benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Erken ödemenin uygulandığı hedef kredinin kimliği.</summary>
    public Guid LoanId { get; init; }

    /// <summary>Erken ödemenin yapıldığı veya yapılacağı kesin tarih.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Erken ödemenin kredinin taksit ve vade yapısına yansıma biçimi.</summary>
    public LoanPrepaymentMode Mode { get; init; }

    /// <summary>
    /// Kısmi ara ödemede anaparadan düşülecek net nakit tutar.
    /// Tam kapama (<see cref="LoanPrepaymentMode.FullClosure"/>) durumunda null'dır;
    /// kapanış anındaki güncel anapara ve yasal tazminat üzerinden hesaplanır.
    /// </summary>
    public decimal? PrincipalAmount { get; init; }
}
