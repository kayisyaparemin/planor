using Mizan.Domain.Calculations;
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
/// <param name="Observations">Kullanıcının dönem içi gözlemleri, güne göre sıralı; hiç bakiye girilmediyse boş (S68-1). Grafikteki noktalar bunlardır.</param>
/// <param name="PaymentMarks">Kullanıcının ödeme satırlarına koyduğu işaretler; gözlemden bağımsızdır (S68-8).</param>
/// <param name="ReminderAnswers">Vadesi bu döneme düşen ödemelere verilmiş "Ödedim" / "Ertele" cevapları.</param>
public sealed record OpenPeriodLedger(
    PeriodPlanSnapshot Plan,
    IReadOnlyList<PeriodPlanRevision> Revisions,
    IReadOnlyList<PeriodObservation> Observations,
    IReadOnlyList<PeriodPaymentMark> PaymentMarks,
    IReadOnlyList<PaymentReminderResponse> ReminderAnswers)
{
    /// <summary>
    /// Gidişatın hesaplandığı son gözlem: en geç tarihli olan, giriş sırası değil (S68-3); gözlem yoksa <c>null</c>.
    /// </summary>
    public PeriodObservation? LatestObservation => PeriodObservationRules.Latest(Observations);

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
