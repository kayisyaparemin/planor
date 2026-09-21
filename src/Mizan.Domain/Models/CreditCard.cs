namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının bir bankaya ait kredi kartı hesabını, limitini, hesap kesim döngüsünü,
/// mevcut borç bakiyesini ve ödeme stratejisi parametrelerini temsil eden sözleşme.
/// Nakit akış projeksiyonunda kart yükümlülüklerinin hesaplanması ve limit takibi için temel kaynaktır.
/// </summary>
public sealed record CreditCard
{
    /// <summary>Kredi kartının benzersiz kimliği.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Kullanıcının karta verdiği ad (örn. Axess Platinum, Bonus Kart).</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Kartın ait olduğu banka veya finans kuruluşu.</summary>
    public string Bank { get; init; } = string.Empty;

    /// <summary>Kartın toplam tahsis edilen kredi limiti.</summary>
    public decimal Limit { get; init; }

    /// <summary>Geçmiş ekstrelerden devreden, faiz işletilmeye devam eden ödenmemiş anapara ve faiz borcu.</summary>
    public decimal CarriedBalance { get; init; }

    /// <summary>Henüz bir ekstreye girmemiş, güncel dönem içi faturalanmamış harcama toplamı.</summary>
    public decimal UnbilledSpending { get; init; }

    /// <summary>Bakiye ve harcama bilgilerinin girildiği veya son mutabakatın yapıldığı referans tarih.</summary>
    public DateOnly BalanceAsOfDate { get; init; }

    /// <summary>Her ay ekstrenin kesildiği hesap kesim günü (1-31).</summary>
    public int StatementClosingDay { get; init; }

    /// <summary>Her ay ekstrenin son ödeme günü (1-31).</summary>
    public int PaymentDueDay { get; init; }

    /// <summary>Yasal veya banka tarafından belirlenen asgari ödeme oranı (örn. 0.20 veya 0.40).</summary>
    public decimal MinimumPaymentRate { get; init; }

    /// <summary>Kullanıcının varsayılan ekstre ödeme stratejisi (Asgari, Tamamı, Sabit veya Her Ekstrede Sor).</summary>
    public CreditCardPaymentStrategy PaymentStrategy { get; init; } = CreditCardPaymentStrategy.AskEachStatement;

    /// <summary>Sabit tutar ödeme stratejisi seçildiğinde her dönem ödenmesi hedeflenen tutar.</summary>
    public decimal? FixedPaymentAmount { get; init; }

    /// <summary>Gelecek dönem simülasyonlarında ekstre ödemesi kestirilemediğinde kullanılacak yedek strateji.</summary>
    public ProjectionFallbackStrategy ProjectionFallbackStrategy { get; init; } = ProjectionFallbackStrategy.None;

    /// <summary>Yedek strateji sabit tutar olduğunda kullanılacak varsayılan tutar.</summary>
    public decimal? ProjectionFallbackFixedAmount { get; init; }

    /// <summary>Banka tarafından kesilmiş en güncel hesap kesim ekstresi.</summary>
    public CreditCardStatement? CurrentStatement { get; init; }

    /// <summary>Kesilmiş mevcut ekstre için kullanıcının belirlediği anlık ödeme tercihi.</summary>
    public CurrentStatementPaymentPlan? CurrentStatementPaymentPlan { get; init; }

    /// <summary>Settlement sonrası bankanın bildirdiği bir sonraki kesin hesap kesim tarihi.</summary>
    public DateOnly? KnownNextStatementDate { get; init; }

    /// <summary>Settlement sonrası bankanın bildirdiği bir sonraki kesin son ödeme tarihi.</summary>
    public DateOnly? KnownNextDueDate { get; init; }

    /// <summary>Kartın aktif olarak nakit akış planında kullanılıp kullanılmadığı.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Karta yansıyacak gelecek taksitler ve bekleyen tarihli harcamalar.</summary>
    public IReadOnlyList<CardCharge> Charges { get; init; } = [];

    /// <summary>Belirli son ödeme tarihleri için tanımlanmış özel ödeme planları.</summary>
    public IReadOnlyList<CreditCardPaymentPlan> PaymentPlans { get; init; } = [];

    /// <summary>Kullanıcının ekstre ödeme tercihlerinin etkin tarihli (effective-dated) tarihçe kayıtları.</summary>
    public IReadOnlyList<CreditCardPaymentPreference> PaymentPreferences { get; init; } = [];

    /// <summary>
    /// Kartın bilinen tüm bileşenlere göre hesaplanan güncel toplam borcu.
    /// Ekstre kesilmişse ekstre tutarı ile ekstre tarihinden sonraki harcamaların toplamıdır;
    /// ekstre yoksa devreden bakiye, dönem içi harcama ve tüm gelecek harcamaların toplamıdır.
    /// </summary>
    public decimal KnownTotalDebt => CurrentStatement is null
        ? CarriedBalance + UnbilledSpending + Charges.Sum(x => x.Amount)
        : CurrentStatement.StatementAmount +
          Charges
              .Where(x => x.PostingDate > CurrentStatement.StatementDate)
              .Sum(x => x.Amount);

    /// <summary>
    /// Kartın toplam limitinden güncel borcun düşülmesiyle bulunan kalan kullanılabilir limit.
    /// Borç limiti aştığında negatif dönebilir.
    /// </summary>
    public decimal AvailableLimit => Limit - KnownTotalDebt;
}
