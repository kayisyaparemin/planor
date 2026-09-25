using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kullanıcının nakit akış parametreleri ve varsayılan faiz oranlarını saklayan
/// <c>settings</c> tablosunun SQLite varlık modeli.
/// </summary>
[Table(DatabaseConstants.TableSettings)]
internal sealed class SettingsEntity
{
    /// <summary>Tekil ayar satırı birincil anahtarı (sabit: 1).</summary>
    [PrimaryKey]
    public int Id { get; set; } = 1;

    /// <summary>Nakit akış dönemlerinin çapa günü (1-31).</summary>
    public int PeriodAnchorDay { get; set; } = 10;

    /// <summary>Dönemlik serbest harcama / yaşam gideri havuzu tutarı.</summary>
    public decimal PeriodVariableExpenseAllowance { get; set; }

    /// <summary>Referans çapa tarihindeki açılış nakit bakiyesi.</summary>
    public decimal ProjectionOpeningBalance { get; set; }

    /// <summary>Projeksiyon başlangıç referans çapa tarihi (yyyy-MM-dd).</summary>
    public string ProjectionAnchorDate { get; set; } = string.Empty;

    /// <summary>Kredi kartı devreden borç aylık akdi faiz oranı.</summary>
    public decimal CreditCardCarryInterestRate { get; set; } = DatabaseConstants.DefaultPlanningInterestRate;

    /// <summary>Dönem içi/sonu nakit açığı (KMH) aylık borçlanma faiz oranı.</summary>
    public decimal DeficitFinancingInterestRate { get; set; } = DatabaseConstants.DefaultPlanningInterestRate;

    /// <summary>Ödeme günü bildirim hatırlatıcı tercih modu (0: Kapalı, 1: Yalnızca vadesi gelenler, vb.).</summary>
    public int PaymentReminderMode { get; set; }
}
