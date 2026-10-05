using Mizan.Application.Models;
using Mizan.Application.Services;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Ana sayfanın "Kalan ödemeler" kartı (S88): satıra dokununca onay sorulur, "Ödedim" hatırlatıcının cevabıyla aynı
/// kaydı yazar; liste "+N daha" ile açıldıysa yeni gidişatta da açık kalır.
/// </summary>
public sealed class RemainingPaymentsViewModelTests : IDisposable
{
    private static readonly DateOnly Start = new(2026, 9, 10);
    private static readonly TimeSpan TurkiyeSaati = TimeSpan.FromHours(3);

    private readonly FakePaymentReminderService _service = new();
    private readonly FakeDialogService _dialog = new();
    private readonly SabitSaat _clock = new(new DateOnly(2026, 9, 30));
    private readonly ProfileService _profileService;
    private readonly ReminderCardViewModel _reminders;
    private readonly RemainingPaymentsViewModel _viewModel;

    public RemainingPaymentsViewModelTests()
    {
        _profileService = new ProfileService(new FakeProfileRepository(), new FakeProfileStoreSwitch(), _clock);
        _reminders = new ReminderCardViewModel(_service, new FakePaymentReminderScheduler(), _dialog, _clock, _profileService);
        _viewModel = new RemainingPaymentsViewModel(_reminders, _dialog);
    }

    [Fact]
    public void Goster_SatirHatirlaticininOdemeAnahtariniTasir()
    {
        _viewModel.Show(Progress(Payment("kira-key", "Kira", new DateOnly(2026, 10, 1), 4000m)));

        var item = Assert.Single(_viewModel.Items);
        Assert.Equal("kira-key", item.DueKey);
        Assert.Equal("Kira", item.Name);
        Assert.Equal(new DateOnly(2026, 10, 1), item.DueDate);
        Assert.Equal(4000m, item.Amount);
        Assert.Equal(1, _viewModel.Count);
        Assert.Equal(4000m, _viewModel.Total);
    }

    [Fact]
    public void Goster_KalanOdemeVarsa_KartGorunur()
    {
        _viewModel.Show(Progress(Payments(1)));

        Assert.True(_viewModel.HasItems);
    }

    [Fact]
    public void Goster_SonKalanOdemeDeOdendiyse_KartGizlenir()
    {
        // Hazırla — tek kalan ödemeye "Ödedim" dendi; yerinde yenileme boş gidişatla gelir (S88-6)
        _viewModel.Show(Progress(Payments(1)));
        var changed = new List<string?>();
        _viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        // Uygula
        _viewModel.Show(Progress());

        // Doğrula — kart bağlaması değişikliği duyar
        Assert.False(_viewModel.HasItems);
        Assert.Contains(nameof(RemainingPaymentsViewModel.HasItems), changed);
    }

    [Fact]
    public void Goster_ListeAcildiysa_YeniGidisattaDaAcikKalir()
    {
        // Hazırla — beş ödemelik liste açıldı; birine "Ödedim" denince gidişat dört ödemeyle yeniden gelir (S88-4)
        _viewModel.Show(Progress(Payments(5)));
        _viewModel.Expand();

        // Uygula
        _viewModel.Show(Progress(Payments(4)));

        // Doğrula
        Assert.Equal(4, _viewModel.Items.Count);
        Assert.Equal(0, _viewModel.HiddenCount);
        Assert.False(_viewModel.HasOverflow);
    }

    [Fact]
    public void Daralt_AcilmisListeIlkUceDoner()
    {
        _viewModel.Show(Progress(Payments(5)));
        _viewModel.Expand();

        _viewModel.Collapse();

        Assert.Equal(RemainingPaymentsViewModel.CollapsedRowLimit, _viewModel.Items.Count);
        Assert.Equal(2, _viewModel.HiddenCount);
        Assert.True(_viewModel.HasOverflow);
    }

    [Fact]
    public async Task OdedimeDokunmak_OnaylanirsaHatirlaticininOdedimCevabiniYerelSaatleYazar()
    {
        // Hazırla — telefon Türkiye saatinde, gece yarısından sonra: cevap yerel saatle yazılır (I168)
        _clock.YerelSaatiAyarla(new DateTimeOffset(2026, 9, 30, 0, 30, 0, TurkiyeSaati));
        _viewModel.Show(Progress(Payment("kira-key", "Kira", new DateOnly(2026, 10, 1), 4000m)));

        // Uygula
        await _viewModel.MarkAsPaidCommand.ExecuteAsync(_viewModel.Items[0]);

        // Doğrula — onayın başlığı ödemenin adı; kayıt kartın "Ödedim"iyle aynı (S88-1, -3)
        Assert.Equal("Kira", _dialog.LastConfirmTitle);
        var answer = Assert.Single(_service.RecordedAnswers);
        Assert.Equal(PaymentReminderAnswerKind.Paid, answer.Kind);
        Assert.Equal(new DateTime(2026, 9, 30, 0, 30, 0), answer.AnsweredAt);
        Assert.Null(answer.SnoozedUntil);
        Assert.Equal([new PaymentDue("kira-key", "Kira", new DateOnly(2026, 10, 1), 4000m)], answer.Payments);
    }

    [Fact]
    public async Task OdedimeDokunmak_OnaylanirsaAnaSayfayaCevabiDuyurur()
    {
        _viewModel.Show(Progress(Payments(1)));
        var answered = false;
        _reminders.AnswersChanged += (_, _) => answered = true;

        await _viewModel.MarkAsPaidCommand.ExecuteAsync(_viewModel.Items[0]);

        Assert.True(answered);
    }

    [Fact]
    public async Task OdedimeDokunmak_VazgecilirseHicbirSeyYazilmaz()
    {
        _dialog.NextConfirmResponse = false;
        _viewModel.Show(Progress(Payments(1)));

        await _viewModel.MarkAsPaidCommand.ExecuteAsync(_viewModel.Items[0]);

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.Empty(_service.RecordedAnswers);
        Assert.Null(_dialog.LastAlertTitle);
    }

    [Fact]
    public async Task OdedimeDokunmak_KayitYazilamazsaUyariGosterirListeDegismez()
    {
        _service.RecordFailure = new InvalidOperationException("Veritabanı kilitli.");
        _viewModel.Show(Progress(Payments(2)));

        await _viewModel.MarkAsPaidCommand.ExecuteAsync(_viewModel.Items[0]);

        Assert.Equal("Ödeme işaretlenemedi", _dialog.LastAlertTitle);
        Assert.Equal(2, _viewModel.Items.Count);
        Assert.Equal(2, _viewModel.Count);
    }

    [Fact]
    public void Temizle_ListeBosalirTasmaKalmaz()
    {
        _viewModel.Show(Progress(Payments(5)));

        _viewModel.Clear();

        Assert.Empty(_viewModel.Items);
        Assert.Equal(0, _viewModel.Count);
        Assert.Equal(0m, _viewModel.Total);
        Assert.False(_viewModel.HasOverflow);
        Assert.False(_viewModel.HasItems);
    }

    private static PeriodRemainingPayment Payment(string dueKey, string name, DateOnly dueDate, decimal amount) =>
        new(Guid.NewGuid(), dueKey, name, dueDate, amount);

    private static PeriodRemainingPayment[] Payments(int count) =>
        Enumerable.Range(0, count)
            .Select(i => Payment($"key-{i}", $"Ödeme {i}", new DateOnly(2026, 10, 1).AddDays(i), 1000m))
            .ToArray();

    private static PeriodProgress Progress(params PeriodRemainingPayment[] payments)
    {
        var end = new DateOnly(2026, 10, 10);
        return new PeriodProgress
        {
            PeriodPlanSnapshotId = Guid.NewGuid(),
            PeriodStart = Start,
            PeriodEnd = end,
            Today = new DateOnly(2026, 9, 30),
            ElapsedDays = 20,
            TotalDays = 30,
            RevisionCount = 0,
            PlannedIncome = 50000m,
            PlannedMandatoryPayments = 15000m,
            PlannedEndingBalance = 43900m,
            PlannedVariableExpenseAllowance = 30000m,
            PlannedDeficitInterest = 0m,
            ObservedLivingSpend = null,
            RemainingVariableExpenseAllowance = null,
            ProjectedDeficitInterest = null,
            ProjectedEndingBalance = null,
            Pace = null,
            Cards = [],
            Observation = null,
            Observations = [],
            Path = new PeriodBalancePath([new(Start, 0m)], [new(Start, 0m), new(end, 43900m)]),
            RemainingPayments = payments,
            IsClosable = false,
            SnoozedLineIds = new HashSet<Guid>()
        };
    }

    public void Dispose() => _profileService.Dispose();
}
