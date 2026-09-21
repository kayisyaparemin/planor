namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının nakit akış dönemlerini, serbest harcama havuzunu ve faiz parametrelerini belirleyen temel ayarlar.
/// Projeksiyon motorunun başlangıç koşullarını ve borç/kredi kartı simülasyon varsayımlarını yönlendirir.
/// </summary>
public sealed record UserSettings
{
    /// <summary>
    /// Nakit akış dönemlerinin döngü kuralını belirleyen dönem çapası (varsayılan: her ayın 10'u).
    /// </summary>
    public PeriodAnchor PeriodAnchor { get; init; } = new(10);

    /// <summary>
    /// Her nakit akış dönemi için ayrılan serbest harcama / yaşam gideri havuzu tutarı.
    /// Takvim ayı değil, dönemin kendi uzunluğu için geçerlidir (S7).
    /// </summary>
    public decimal PeriodVariableExpenseAllowance { get; init; }

    /// <summary>
    /// Projeksiyon başlangıç tarihindeki (<see cref="ProjectionAnchorDate"/>) açılış nakit bakiyesi.
    /// Kullanıcının başlangıç anındaki mevcut nakit veya eksi bakiye/KMH tutarını temsil eder.
    /// </summary>
    public decimal ProjectionOpeningBalance { get; init; }

    /// <summary>
    /// Projeksiyon ve nakit akış hesaplamalarının başladığı referans çapa tarihi.
    /// </summary>
    public DateOnly ProjectionAnchorDate { get; init; }

    /// <summary>
    /// Kredi kartı ekstre borcunun asgari tutar üzerinde kalan kısmına uygulanacak aylık akdi faiz oranı (varsayılan: %5 / 0.05).
    /// </summary>
    public decimal CreditCardCarryInterestRate { get; init; } = 0.05m;

    /// <summary>
    /// Dönem içi veya dönem sonu nakit açığı (negatif bakiye / KMH) durumunda uygulanacak aylık borçlanma faiz oranı (varsayılan: %5 / 0.05).
    /// </summary>
    public decimal DeficitFinancingInterestRate { get; init; } = 0.05m;
}
