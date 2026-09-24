using Mizan.Domain.Models;

namespace Mizan.Application.Models;

/// <summary>
/// Açık dönemin defteri: dönem başında dondurulan plan, dönem içindeki plan revizyonları,
/// kullanıcının gözlem defteri ve bu döneme ait hatırlatıcı cevapları bir arada.
/// Mevcut dönemin gidişatı bu dört kaynağı birlikte okur. Eskide hepsi tanrı arayüz
/// <c>IMizanStore</c>'dan geliyordu; dar portlarla tek tek okunduklarında gidişat servisi
/// beş bağımlılık sınırını (M3) aşıyordu. Defter, "açık dönemde ne kayıtlı" sorusunu tek yerde cevaplar.
/// </summary>
/// <param name="Plan">Dönem başında dondurulan, değişmeyen plan taahhüdü (I23).</param>
/// <param name="Revisions">Planın dönem içi revizyonları, en eskiden en yeniye (I24).</param>
/// <param name="Observation">Kullanıcının dönem içi gözlem defteri; hiç bakiye girilmediyse <c>null</c>.</param>
/// <param name="ReminderAnswers">Vadesi bu döneme düşen ödemelere verilmiş "Ödedim" / "Ertele" cevapları.</param>
public sealed record OpenPeriodLedger(
    PeriodPlanSnapshot Plan,
    IReadOnlyList<PeriodPlanRevision> Revisions,
    PeriodObservation? Observation,
    IReadOnlyList<PaymentReminderResponse> ReminderAnswers)
{
    /// <summary>
    /// Dönem içinde "planım şu an ne" sorusunun cevabı olan son revizyon; revizyon yoksa <c>null</c>
    /// ve cevap dondurulan planın kendisidir (I24).
    /// </summary>
    public PeriodPlanRevision? LatestRevision => Revisions.Count > 0 ? Revisions[^1] : null;

    /// <summary>
    /// Dönem içinde "planım şu an ne" sorusunun ödeme satırları: son revizyonunkiler, revizyon
    /// yoksa dondurulan planınkiler (I24). Dondurulan plan tarihçede değişmeden kalır.
    /// </summary>
    public IReadOnlyList<PeriodPlanPaymentLine> CurrentPaymentLines =>
        LatestRevision?.PaymentLines ?? Plan.PaymentLines;

    /// <summary>
    /// Dönem içinde "planım şu an ne" sorusunun gelir satırları, yatacakları günlerle: son
    /// revizyonunkiler, revizyon yoksa dondurulan planınkiler (I24, S31).
    /// </summary>
    public IReadOnlyList<PeriodPlanIncomeLine> CurrentIncomeLines =>
        LatestRevision?.IncomeLines ?? Plan.IncomeLines;
}
