using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Açık dönemin bakiye rotasını (<see cref="PeriodBalancePath"/>) üretir (S71). Kullanıcı yalnız birkaç günde
/// bakiye girer; aradaki ve sonraki günler planın gelir ve ödeme günlerinden çizilir, hareketlerin açıklayamadığı
/// fark (yaşam harcaması) günlere eşit yayılır. Gidişat yalnız dönem sonunu söyler; bu sınıf aynı terimleri
/// günlere dağıtır ve son noktası gidişatın rakamına kuruşu kuruşuna eşittir. Hiçbir şey yazmaz.
/// </summary>
public static class PeriodBalancePathCalculator
{
    private static readonly Dictionary<Guid, decimal> EmptyCardPayments = [];

    /// <summary>
    /// Gözlem yokken rota saf plandır (S71-6): açılış bakiyesinden, güncel planın gelir ve ödeme satırları
    /// planlanan gün ve tutarlarıyla, yaşam havuzunun tamamı dönem boyunca eşit, planlanan KMH faizi son noktada.
    /// Son noktası planlanan kapanıştır.
    /// </summary>
    /// <param name="ledger">Açık dönemin planı ve revizyonları.</param>
    public static PeriodBalancePath FromPlan(OpenPeriodLedger ledger)
    {
        var plan = ledger.Plan;
        var latest = ledger.LatestRevision;
        var opening = new BalancePathPoint(plan.PeriodStart, plan.OpeningBalance);
        var ahead = Ahead(
            opening,
            PlannedMovements(ledger, unpaidLineIds: [], EmptyCardPayments),
            latest?.PlannedVariableExpenseAllowance ?? plan.PlannedVariableExpenseAllowance,
            latest?.PlannedDeficitInterest ?? plan.PlannedDeficitInterest,
            plan.PeriodEnd);
        return new PeriodBalancePath([opening], ahead);
    }

    /// <summary>
    /// Gözlem varken rota (S71-4, 5): katedilen yol gözlemlerden geçer, aralar plandan çizilir; önümüzdeki yol
    /// son gözlemden gidişatın terimleriyle dönem sonuna iner. Son noktası dönem sonu tahminidir.
    /// </summary>
    /// <param name="ledger">Açık dönemin defteri; en az bir gözlem taşır.</param>
    /// <param name="lines">Ödeme satırlarının son gözleme göre durumu.</param>
    /// <param name="currentCardPayments">Kart kimliğine göre kartın bugünkü hâliyle bu dönemde vadesi gelen ödeme (I23).</param>
    /// <param name="remainingAllowance">Gidişatın kalan yaşam havuzu; son gözlemden dönem sonuna eşit yayılır.</param>
    /// <param name="deficitInterest">Gidişatın dönem sonu KMH faizi; son noktada düşülür.</param>
    /// <exception cref="ArgumentException">Defterde gözlem yoksa; o zaman rota <see cref="FromPlan"/>'dır.</exception>
    public static PeriodBalancePath FromObservations(
        OpenPeriodLedger ledger,
        PeriodPaymentLineClassification lines,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments,
        decimal remainingAllowance,
        decimal deficitInterest)
    {
        var latest = ledger.LatestObservation ??
            throw new ArgumentException("Gözlemsiz dönemin rotası plandan çizilir.", nameof(ledger));
        var travelled = Travelled(ledger, lines, currentCardPayments);
        var ahead = Ahead(
            travelled[^1],
            MovementsAfter(latest.ObservedOn, ledger, lines, currentCardPayments),
            remainingAllowance,
            deficitInterest,
            ledger.Plan.PeriodEnd);
        return new PeriodBalancePath(travelled, ahead);
    }

    // Ödenmemiş satırlar (ertelenen, "ödenmedi" işaretli) katedilen yolda düşülmez; önümüzdeki yolda düşerler.
    private static Movement[] PlannedMovements(
        OpenPeriodLedger ledger,
        HashSet<Guid> unpaidLineIds,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments) =>
        ledger.CurrentIncomeLines
            .Select(x => new Movement(x.PlannedDate, x.PlannedAmount, IsIncome: true))
            .Concat(ledger.CurrentPaymentLines
                .Where(x => !unpaidLineIds.Contains(x.Id))
                .Select(x => new Movement(x.PlannedDate, -ProjectedPaymentAmount.Of(x, currentCardPayments), IsIncome: false)))
            .ToArray();

    // Son gözlemde görünmeyen hareketler, gidişatın terimleriyle birebir: gözlemden sonra yatacak gelir, gözlemden
    // sonra yapılmış ödemeler (günleri bilinmez, gözlem gününe düşer) ve kalan satırlar kartın bugünkü tutarıyla.
    private static Movement[] MovementsAfter(
        DateOnly observedOn,
        OpenPeriodLedger ledger,
        PeriodPaymentLineClassification lines,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments) =>
        ledger.CurrentIncomeLines
            .Where(x => x.PlannedDate > observedOn)
            .Select(x => new Movement(x.PlannedDate, x.PlannedAmount, IsIncome: true))
            .Append(new Movement(observedOn, -lines.SettledAfterObservation, IsIncome: false))
            .Concat(lines.RemainingLines.Select(x =>
                new Movement(x.PlannedDate, -ProjectedPaymentAmount.Of(x, currentCardPayments), IsIncome: false)))
            .ToArray();

    private static List<BalancePathPoint> Travelled(
        OpenPeriodLedger ledger,
        PeriodPaymentLineClassification lines,
        IReadOnlyDictionary<Guid, decimal> currentCardPayments)
    {
        var anchors = Anchors(ledger);
        var movements = PlannedMovements(ledger, lines.RemainingLines.Select(x => x.Id).ToHashSet(), currentCardPayments);
        var points = new List<BalancePathPoint> { anchors[0].Point };
        for (var i = 1; i < anchors.Count; i++)
        {
            points.AddRange(Between(anchors[i - 1], anchors[i], movements));
        }

        return points;
    }

    // Bağ noktaları: açılış ve gözlemler, güne göre. Dönem başına düşen gözlem açılışın yerine geçer. Dönem dışına
    // tarihli eski gözlem en yakın güne çekilir; aynı güne düşenlerden en geç tarihlisi kalır (S71 açık not a).
    private static List<Anchor> Anchors(OpenPeriodLedger ledger)
    {
        var plan = ledger.Plan;
        var anchors = ledger.Observations
            .GroupBy(x => Clamp(x.ObservedOn, plan.PeriodStart, plan.PeriodEnd.AddDays(-1)))
            .Select(day => new Anchor(day.Key, day.MaxBy(x => x.ObservedOn)!.ObservedBalance, IsObservation: true))
            .OrderBy(x => x.Day)
            .ToList();
        if (anchors.Count == 0 || anchors[0].Day != plan.PeriodStart)
        {
            anchors.Insert(0, new Anchor(plan.PeriodStart, plan.OpeningBalance, IsObservation: false));
        }

        return anchors;
    }

    // İki bağ noktası arası (S71-4): sonrakinin içerdiği ama öncekinin içermediği hareketler kendi günlerinde,
    // açıklanamayan fark günlere eşit yayılır; çizgi iki noktaya da tam oturur.
    private static IEnumerable<BalancePathPoint> Between(Anchor from, Anchor to, IReadOnlyList<Movement> movements)
    {
        var between = movements.Where(x => to.Contains(x) && !from.Contains(x)).ToArray();
        var unexplained = from.Balance + between.Sum(x => x.Amount) - to.Balance;
        var days = to.Day.DayNumber - from.Day.DayNumber;
        for (var day = from.Day.AddDays(1); day < to.Day; day = day.AddDays(1))
        {
            var elapsed = day.DayNumber - from.Day.DayNumber;
            var moved = between.Where(x => x.Day < day).Sum(x => x.Amount);
            yield return new BalancePathPoint(day, Round(from.Balance + moved - unexplained * elapsed / days));
        }

        yield return to.Point;
    }

    // Önümüzdeki yol (S71-5, 6): hareket ertesi günün noktasında görünür, havuz dönem sonuna eşit yayılır, KMH faizi
    // son noktada. Hareket bağ noktasından önceye ya da son günden sonraya düşmez; son nokta bütün terimleri içerir.
    private static List<BalancePathPoint> Ahead(
        BalancePathPoint anchor,
        IReadOnlyList<Movement> movements,
        decimal spreadAllowance,
        decimal deficitInterest,
        DateOnly periodEnd)
    {
        var lastDay = periodEnd.AddDays(-1);
        var days = periodEnd.DayNumber - anchor.Date.DayNumber;
        var points = new List<BalancePathPoint> { anchor };
        for (var day = anchor.Date.AddDays(1); day <= periodEnd; day = day.AddDays(1))
        {
            var elapsed = day.DayNumber - anchor.Date.DayNumber;
            var moved = movements.Where(x => Clamp(x.Day, anchor.Date, lastDay) < day).Sum(x => x.Amount);
            var interest = day == periodEnd ? deficitInterest : 0m;
            points.Add(new BalancePathPoint(day, Round(anchor.Balance + moved - spreadAllowance * elapsed / days - interest)));
        }

        return points;
    }

    private static DateOnly Clamp(DateOnly day, DateOnly first, DateOnly last) =>
        day < first ? first : day > last ? last : day;

    private static decimal Round(decimal balance) => Math.Round(balance, 2, MidpointRounding.AwayFromZero);

    // Bir gelir ya da ödeme: kendi gününde olur, ertesi günün başındaki bakiyede görünür.
    private readonly record struct Movement(DateOnly Day, decimal Amount, bool IsIncome);

    // Rotanın kesin bildiği bakiye: açılış ya da gözlem. Gözlem o günün gelirini içerir, ödemesini içermez
    // (S31, S68-8); açılış o günün hiçbir hareketini içermez.
    private sealed record Anchor(DateOnly Day, decimal Balance, bool IsObservation)
    {
        public BalancePathPoint Point => new(Day, Balance);

        public bool Contains(Movement movement) =>
            movement.Day < Day || (IsObservation && movement.IsIncome && movement.Day == Day);
    }
}
