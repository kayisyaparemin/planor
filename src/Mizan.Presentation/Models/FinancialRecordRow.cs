namespace Mizan.Presentation.Models;

/// <summary>
/// Finansal Yapı listesinde bir kaydın ham satırı. Ekranın "ne kadar, sıradaki ne zaman" sorusunu
/// sayı ve tarihle taşır; bağlam cümlesini App'teki çevirici türüne göre kurar (EK-V6, kural: ham veri).
/// </summary>
/// <param name="Id">Kaydın kimliği; silme ve kart kontrole gitme bununla yapılır.</param>
/// <param name="Kind">Kaydın türü.</param>
/// <param name="Name">Kaydın adı; ad boşsa banka adı.</param>
/// <param name="Amount">Satırın tutarı (gelir, sıradaki ödeme, taksit); bilinmiyorsa null.</param>
/// <param name="NextDate">Sıradaki ödeme ya da tek seferlik kaydın tarihi; yoksa null.</param>
public sealed record FinancialRecordRow(
    Guid Id,
    FinancialRecordKind Kind,
    string Name,
    decimal? Amount,
    DateOnly? NextDate)
{
    /// <summary>Düzenli gelirin her ay geçtiği gün.</summary>
    public int? DayOfMonth { get; init; }

    /// <summary>Kredide kalan taksit, planda kalan ödeme sayısı.</summary>
    public int? RemainingCount { get; init; }

    /// <summary>Satırda tutar gösterilip gösterilmeyeceği.</summary>
    public bool HasAmount => Amount is not null;
}
