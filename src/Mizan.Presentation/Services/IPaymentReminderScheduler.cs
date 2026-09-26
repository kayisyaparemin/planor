using Mizan.Application.Models;

namespace Mizan.Presentation.Services;

/// <summary>
/// İşletim sistemi bildirim alarmlarını kurmak, iptal etmek ve bildirim üzerinden
/// gelen kullanıcı yanıtlarını okumak için gereken platform servis portudur.
/// </summary>
public interface IPaymentReminderScheduler
{
    /// <summary>Profil için hesaplanan hatırlatıcı bildirimlerini işletim sisteminde zamanlar.</summary>
    void Schedule(Guid profileId, IReadOnlyList<PaymentReminder> reminders);

    /// <summary>Bildirim düğmeleriyle verilen ve henüz işlenmemiş yanıtları okur.</summary>
    IReadOnlyList<PaymentReminderAnswer> ReadPendingAnswers(Guid profileId);

    /// <summary>İşlenen yanıtları işletim sistemi kuyruğundan onaylayarak kaldırır.</summary>
    void AcknowledgeAnswers(Guid profileId, int count);

    /// <summary>Profilin işletim sisteminde kayıtlı tüm bildirim alarmlarını iptal eder.</summary>
    void CancelAll(Guid profileId);
}
