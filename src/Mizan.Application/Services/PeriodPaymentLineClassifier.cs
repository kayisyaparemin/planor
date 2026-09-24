using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Açık dönemin ödeme satırlarından hangisinin yapıldığını, hangisinin kaldığını bulur.
/// Kullanıcıdan her ödemeyi tek tek işaretlemesi beklenmez; üç kaynak öncelik sırasıyla okunur:
/// gözlem defterindeki açık işaret, hatırlatıcıya verilen "Ödedim" / "Ertele" cevabı ve son olarak
/// vade — vadesi gelen ödeme yapılmış sayılır, planın kendi varsayımı da budur.
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
    /// <param name="today">Vadesi gelen ödemelerin yapılmış sayılacağı gün.</param>
    public static PeriodPaymentLineClassification Classify(OpenPeriodLedger ledger, DateOnly today)
    {
        var marks = ExplicitMarksByDueKey(ledger);
        var answers = LatestAnswersByDueKey(ledger.ReminderAnswers);
        var outcomes = ledger.CurrentPaymentLines
            .Select(line => FromExplicitMark(line, marks)
                            ?? FromReminderAnswer(line, answers, ledger.Observation)
                            ?? FromDueDate(line, ledger.Observation, today))
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

    // Açık işaret gözlem defterinin parçasıdır; bu yüzden bakiyeye yansımış sayılır ve planlanan
    // değil, fiilen ödenen tutar düşülür.
    private static LineOutcome? FromExplicitMark(
        PeriodPlanPaymentLine line,
        Dictionary<string, PeriodObservationPayment> marks)
    {
        if (!marks.TryGetValue(DueKey(line), out var mark))
        {
            return null;
        }

        return mark.IsSettled
            ? new LineOutcome(line, LineState.SettledBeforeObservation, mark.ActualAmount)
            : new LineOutcome(line, LineState.Remaining, 0m);
    }

    // "Ertele" denen ödeme vadesi geçse de kalandır. "Ödedim" cevabı gözlemden önce verildiyse
    // ödeme bakiyeye yansımıştır, sonra verildiyse bakiyede henüz görünmez.
    private static LineOutcome? FromReminderAnswer(
        PeriodPlanPaymentLine line,
        Dictionary<string, PaymentReminderResponse> answers,
        PeriodObservation? observation)
    {
        if (!answers.TryGetValue(DueKey(line), out var answer))
        {
            return null;
        }

        if (answer.Kind != PaymentReminderAnswerKind.Paid)
        {
            return new LineOutcome(line, LineState.Snoozed, 0m);
        }

        return Settled(line, observation is null || AnsweredAtUtc(answer) <= observation.UpdatedAtUtc.UtcDateTime);
    }

    // Gözlem günü düşen ödeme bakiyeye henüz yansımamış sayılır: yanılırsak dönem sonu kötümser çıkar.
    private static LineOutcome FromDueDate(PeriodPlanPaymentLine line, PeriodObservation? observation, DateOnly today)
    {
        if (line.PlannedDate > today)
        {
            return new LineOutcome(line, LineState.Remaining, 0m);
        }

        return Settled(line, observation is null || line.PlannedDate < observation.ObservedOn);
    }

    private static LineOutcome Settled(PeriodPlanPaymentLine line, bool isReflectedInObservedBalance) =>
        new(
            line,
            isReflectedInObservedBalance ? LineState.SettledBeforeObservation : LineState.SettledAfterObservation,
            line.PlannedAmount ?? 0m);

    // Plan sürümleri eskiden yeniye dolaşılır; aynı ödemeye birden fazla sürümde işaret konmuşsa
    // en yeni sürümdeki üzerine yazılarak kalır (S33).
    private static Dictionary<string, PeriodObservationPayment> ExplicitMarksByDueKey(OpenPeriodLedger ledger)
    {
        var marks = new Dictionary<string, PeriodObservationPayment>(StringComparer.Ordinal);
        if (ledger.Observation is not { } observation)
        {
            return marks;
        }

        var linesOldestFirst = ledger.Revisions
            .Select(x => x.PaymentLines)
            .Prepend(ledger.Plan.PaymentLines)
            .SelectMany(x => x);
        foreach (var line in linesOldestFirst)
        {
            if (observation.FindPayment(line.Id) is { } mark)
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

    // Cevap zamanı yerel DateTime, gözlem zamanı UTC; türü belirsiz saat yerel sayılır.
    // AnsweredAt'in DateTimeOffset'e geçmesi kural 05 gereği ayrı bir düzeltmedir (A15a notu).
    private static DateTime AnsweredAtUtc(PaymentReminderResponse answer) => answer.AnsweredAt.ToUniversalTime();

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
