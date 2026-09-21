namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının simülatör ekranında tanımladığı tekil bir varsayımsal senaryo koşulu sözleşmesi.
/// Mevcut finansal plana geçici olarak enjekte edilecek harcama, borçlanma veya gelir değişikliklerini taşır.
/// </summary>
public sealed record SimulationRequest
{
    /// <summary>
    /// Varsayılan yapıcı. Nesne ilklendirici (object initializer) ile kullanım içindir.
    /// </summary>
    public SimulationRequest()
    {
    }

    /// <summary>
    /// Yaygın kullanım için temel alanları ilklendiren yardımcı yapıcı.
    /// </summary>
    public SimulationRequest(
        SimulationScenarioType type,
        string name,
        decimal amount,
        DateOnly startDate,
        int paymentCount = 1)
    {
        Type = type;
        Name = name;
        Amount = amount;
        StartDate = startDate;
        PaymentCount = paymentCount;
    }

    /// <summary>Senaryo koşulunun türü.</summary>
    public SimulationScenarioType Type { get; init; }

    /// <summary>Senaryo koşulunun kullanıcıya gösterilecek açıklayıcı adı.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>İşleme konu olan parasal tutar.</summary>
    public decimal Amount { get; init; }

    /// <summary>Senaryonun yürürlüğe gireceği veya işlemin yapılacağı başlangıç tarihi.</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Taksitli veya periyodik ödemelerdeki toplam ödeme adedi (varsayılan 1).</summary>
    public int PaymentCount { get; init; } = 1;

    /// <summary>İlk taksit veya geri ödemenin vadesi (belirtilmezse StartDate kullanılır).</summary>
    public DateOnly? FirstPaymentDate { get; init; }

    /// <summary>Kart harcaması veya kart ödeme modu senaryolarında hedef kredi kartı kimliği.</summary>
    public Guid? CreditCardId { get; init; }

    /// <summary>Finansman kredisinde faiz ve masraflar dahil toplam geri ödenecek tutar.</summary>
    public decimal? TotalRepaymentAmount { get; init; }

    /// <summary>Düzenli gelir artışı senaryosunda hedef gelir akışının kimliği.</summary>
    public Guid? RecurringIncomeId { get; init; }

    /// <summary>Senaryonun benzersiz kimliği (varsayılan yeni GUID).</summary>
    public Guid ScenarioId { get; init; } = Guid.NewGuid();

    /// <summary>Kredi kartı ödeme modu senaryolarında uygulanacak ödeme türü (Asgari veya Tamamı).</summary>
    public CreditCardPaymentType? CardPaymentType { get; init; }

    /// <summary>Kart ödeme modunun yalnızca ilgili vadeye mi yoksa kartın kalıcı stratejisine mi uygulanacağı.</summary>
    public bool AppliesToAllStatements { get; init; }

    /// <summary>Kredi erken kapama veya ara ödeme senaryolarında hedef kredi sözleşmesi kimliği.</summary>
    public Guid? LoanId { get; init; }

    /// <summary>Kredi ara ödemesinde vadenin mi yoksa aylık taksitin mi azaltılacağı tercihi.</summary>
    public LoanPrepaymentMode? PrepaymentMode { get; init; }
}
