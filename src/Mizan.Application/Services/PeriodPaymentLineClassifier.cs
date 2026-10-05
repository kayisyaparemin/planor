using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Açık dönemin ödeme satırlarından hangisinin yapıldığını, hangisinin kaldığını bulur.
/// Kullanıcıdan her ödemeyi tek tek işaretlemesi beklenmez; üç kaynak öncelik sırasıyla okunur:
/// plana konmuş açık işaret, hatırlatıcıya verilen "Ödedim" / "Ertele" cevabı ve son olarak
/// vade — vadesi gelen ödeme yapılmış sayılır, planın kendi varsayımı da budur.
/// Yapılmış sayılan ödeme kalan ödemeyle aynı tutarla sayılır (<see cref="ProjectedPaymentAmount"/>): kart satırı
/// vade günü kalandan yapılmışa geçerken dondurulan tahmine dönerse dönem sonu tahmini o gün sıçrar (I166).
/// İşaret de cevap da satır kimliğine değil, ödemenin kaynağına ve vadesine bağlanır: plan revizyonu
/// satırlara yeni kimlik verdiğinde kaybolmazlar (I22, S33).
/// </summary>
public static class PeriodPaymentLineClassifier
{
    /// <summary>
    /// Defterin güncel ödeme satırlarını (<see cref="OpenPeriodLedger.CurrentPaymentLines"/>)
    /// verilen güne göre sınıflandırır ve yapılan ödemeleri gözlenen bakiyeye yansıyıp yansımadığına
    /// göre ikiye ayırır.
    /// </summary>
    /// <param name="ledger">Açık dönemin planı, revizyonları, gözlemi ve hatırlatıcı cevapları.</param>
    /// <param name="currentCardPayments">Kart kimliğine göre kartın bugünkü hâliyle bu dönemde vadesi gelen ödeme.</param>
    /// <param name="today">Vadesi gelen ödemelerin yapılmış sayılacağı gün.</param>
    public static PeriodPaymentLineClassification Classify(
        OpenPeriodLedger ledger,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments,
        DateOnly today)
    {
        var marks = ExplicitMarksByDueKey(ledger);
        var answers = LatestAnswersByDueKey(ledger.ReminderAnswers);
        var observation = ledger.LatestObservation;
        var outcomes = ledger.CurrentPaymentLines
            .Select(line => FromExplicitMark(line, marks, observation)
                            ?? FromReminderAnswer(line, answers, observation, currentCardPayments)
                            ?? FromDueDate(line, observation, currentCardPayments, today))
            .ToArray();

        return new PeriodPaymentLineClassification
        {
            SettledBeforeObservation = Total(outcomes, LineState.SettledBeforeObservation),
            SettledAfterObservation = Total(outcomes, LineState.SettledAfterObservation),
            RemainingLines = outcomes
                .Where(x => x.State is LineState.Remaining or LineState.Snoozed)
                .Select(x => x.Line)
                .OrderBy(x => x.PlannedDate)
                .ThenBy(x => x.Name, StringComparer.Ordinal)
                .ToArray(),
            SnoozedLineIds = outcomes
                .Where(x => x.State == LineState.Snoozed)
                .Select(x => x.Line.Id)
                .ToHashSet()
        };
    }

    // Açık işaretin ödeme günü son gözlem gününden önceyse ödeme bakiyeye yansımıştır; aynı gün ya da sonraysa
    // yansımamıştır (S68-8, vade kuralıyla aynı yön). Planlanan değil, fiilen ödenen tutar düşülür.
    private static LineOutcome? FromExplicitMark(
        PeriodPlanPaymentLine line,
        Dictionary<string, PeriodPaymentMark> marks,
        PeriodObservation? observation)
    {
        if (!marks.TryGetValue(DueKey(line), out var mark))
        {
            return null;
        }

        if (!mark.IsSettled)
        {
            return new LineOutcome(line, LineState.Remaining, 0m);
        }

        var isReflected = observation is null || mark.ActualPaymentDate < observation.ObservedOn;
        return new LineOutcome(
            line,
            isReflected ? LineState.SettledBeforeObservation : LineState.SettledAfterObservation,
            mark.ActualAmount);
    }

    // "Ertele" denen ödeme vadesi geçse de kalandır. "Ödedim" cevabı gözlem gününden önceki bir günde verildiyse
    // ödeme bakiyeye yansımıştır; aynı gün ya da sonra verildiyse bakiyede henüz görünmez (kötümser). Kayıt zamanına
    // bakılmaz: geriye tarihli gözlemin kayıt zamanı bugündür ve cevabı yanlış tarafa atardı (S68 açık not b).
    private static LineOutcome? FromReminderAnswer(
        PeriodPlanPaymentLine line,
        Dictionary<string, PaymentReminderResponse> answers,
        PeriodObservation? observation,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments)
    {
        if (!answers.TryGetValue(DueKey(line), out var answer))
        {
            return null;
        }

        if (answer.Kind != PaymentReminderAnswerKind.Paid)
        {
            return new LineOutcome(line, LineState.Snoozed, 0m);
        }

        var isReflected = observation is null || AnsweredOn(answer) < observation.ObservedOn;
        return Settled(line, isReflected, currentCardPayments);
    }

    // Gözlem günü düşen ödeme bakiyeye henüz yansımamış sayılır: yanılırsak dönem sonu kötümser çıkar.
    private static LineOutcome FromDueDate(
        PeriodPlanPaymentLine line,
        PeriodObservation? observation,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments,
        DateOnly today)
    {
        if (line.PlannedDate > today)
        {
            return new LineOutcome(line, LineState.Remaining, 0m);
        }

        return Settled(line, observation is null || line.PlannedDate < observation.ObservedOn, currentCardPayments);
    }

    private static LineOutcome Settled(
        PeriodPlanPaymentLine line,
        bool isReflectedInObservedBalance,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments) =>
        new(
            line,
            isReflectedInObservedBalance ? LineState.SettledBeforeObservation : LineState.SettledAfterObservation,
            ProjectedPaymentAmount.Of(line, currentCardPayments));

    // Plan sürümleri eskiden yeniye dolaşılır; aynı ödemeye birden fazla sürümde işaret konmuşsa
    // en yeni sürümdeki üzerine yazılarak kalır (S33).
    private static Dictionary<string, PeriodPaymentMark> ExplicitMarksByDueKey(OpenPeriodLedger ledger)
    {
        var marks = new Dictionary<string, PeriodPaymentMark>(StringComparer.Ordinal);
        var linesOldestFirst = ledger.Revisions
            .Select(x => x.PaymentLines)
            .Prepend(ledger.Plan.PaymentLines)
            .SelectMany(x => x);
        foreach (var line in linesOldestFirst)
        {
            if (ledger.PaymentMarks.FirstOrDefault(x => x.PeriodPlanPaymentLineId == line.Id) is { } mark)
            {
                marks[DueKey(line)] = mark;
            }
        }

        return marks;
    }

    private static Dictionary<string, PaymentReminderResponse> LatestAnswersByDueKey(
        IReadOnlyList<PaymentReminderResponse> answers) =>
        answers
            .GroupBy(x => x.DueKey, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.OrderBy(AnsweredAtUtc).Last(), StringComparer.Ordinal);

    // Cevap zamanı yerel DateTime; türü belirsiz saat yerel sayılır. AnsweredAt'in DateTimeOffset'e geçmesi
    // kural 05 gereği ayrı bir düzeltmedir (A15a notu).
    private static DateTime AnsweredAtUtc(PaymentReminderResponse answer) => answer.AnsweredAt.ToUniversalTime();

    private static DateOnly AnsweredOn(PaymentReminderResponse answer) =>
        DateOnly.FromDateTime(answer.AnsweredAt.Kind == DateTimeKind.Utc ? answer.AnsweredAt.ToLocalTime() : answer.AnsweredAt);

    private static string DueKey(PeriodPlanPaymentLine line) =>
        PaymentReminderPlanner.DueKey(line.SourceEntityId, line.Name, line.PlannedDate);

    private static decimal Total(IEnumerable<LineOutcome> outcomes, LineState state) =>
        outcomes.Where(x => x.State == state).Sum(x => x.SettledAmount);

    private enum LineState
    {
        Remaining,
        Snoozed,
        SettledBeforeObservation,
        SettledAfterObservation
    }

    private readonly record struct LineOutcome(PeriodPlanPaymentLine Line, LineState State, decimal SettledAmount);
}
