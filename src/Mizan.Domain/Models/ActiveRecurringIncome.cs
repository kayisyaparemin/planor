namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir referans tarih (dönem başlangıcı vb.) itibarıyla çözümlenmiş ve geçerli olan aktif düzenli gelir kalemini temsil eder.
/// Projeksiyon hesaplamalarında ve nakit akış dönemlerinde gelir girişlerinin oluşturulmasında kullanılır.
/// </summary>
public sealed record ActiveRecurringIncome
{
    /// <summary>Kaynak düzenli gelir akışının kimliği.</summary>
    public Guid RecurringIncomeId { get; init; }

    /// <summary>Gelir akışının adı.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gelirin her ay hesaba geçtiği gün (1-31).</summary>
    public int PaymentDay { get; init; }

    /// <summary>Referans tarih itibarıyla geçerli olan net gelir tutarı.</summary>
    public decimal Amount { get; init; }

    /// <summary>Geçerli olan tutarın yürürlüğe girdiği etkin başlangıç tarihi.</summary>
    public DateOnly EffectiveDate { get; init; }
}
