using Mizan.Domain.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Elle ekstre giriş formunun açılış değerlerini, Türkçe tutar okumayı, yeni ekstrenin ödeme
/// şeklinin kartın varsayılanından gelmesini ve hata hâllerini doğrulayan birim testleri (S61).
/// </summary>
public sealed class StatementEntryViewModelTests
{
    private static readonly DateOnly DefaultStatementDate = new(2026, 10, 15);
    private static readonly DateOnly DefaultDueDate = new(2026, 10, 25);

    private readonly FakeCreditCardObligationService _cardService = new();
    private readonly FakeDialogService _dialog = new();
    private readonly StatementEntryViewModel _entry;
    private int _savedCallbackCount;

    public StatementEntryViewModelTests()
    {
        _entry = new StatementEntryViewModel(_cardService, _dialog, () =>
        {
            _savedCallbackCount++;
            return Task.CompletedTask;
        });
    }

    [Fact]
    public void Open_EkstreYoksa_VarsayilanTarihlerleBosAcilir()
    {
        _entry.Open(CreateCard(), DefaultStatementDate, DefaultDueDate);

        Assert.True(_entry.IsOpen);
        Assert.Equal(DefaultStatementDate, _entry.StatementDate);
        Assert.Equal(DefaultDueDate, _entry.DueDate);
        Assert.Equal(string.Empty, _entry.AmountInput);
        Assert.Equal(string.Empty, _entry.MinimumInput);
    }

    [Fact]
    public void Open_EkstreVarsa_AlanlariEkstredenDoldurur()
    {
        var card = CreateCard(statement: CreateStatement(18200.5m, 3640m));

        _entry.Open(card, DefaultStatementDate, DefaultDueDate);

        Assert.Equal(new DateOnly(2026, 9, 15), _entry.StatementDate);
        Assert.Equal(new DateOnly(2026, 9, 25), _entry.DueDate);
        Assert.Equal("18200,5", _entry.AmountInput);
        Assert.Equal("3640", _entry.MinimumInput);
    }

    [Fact]
    public async Task Save_TurkceTutarlariOkurVeEkstreyiKaydeder()
    {
        var card = CreateCard();
        _entry.Open(card, DefaultStatementDate, DefaultDueDate);
        _entry.AmountInput = "18.200,50";
        _entry.MinimumInput = "3.640";

        await _entry.SaveCommand.ExecuteAsync(null);

        var saved = Assert.NotNull(_cardService.LastSavedStatement);
        Assert.Equal(card.Id, saved.CardId);
        Assert.Equal(card.Id, saved.Statement.CreditCardId);
        Assert.Equal(18200.50m, saved.Statement.StatementAmount);
        Assert.Equal(3640m, saved.Statement.MinimumPaymentAmount);
        Assert.Equal(DefaultStatementDate, saved.Statement.StatementDate);
        Assert.Equal(DefaultDueDate, saved.Statement.DueDate);
        Assert.False(_entry.IsOpen);
        Assert.Equal(1, _savedCallbackCount);
    }

    // Klavye yerel ayarı Türkçe değilse tek ondalık işareti nokta olur; "5000.50" 500.050 okunmamalı.
    [Theory]
    [InlineData("18.200,50", 18200.50)]
    [InlineData("18.200", 18200)]
    [InlineData("1.250.000", 1250000)]
    [InlineData("5000.50", 5000.50)]
    [InlineData("5000.5", 5000.5)]
    [InlineData("5000,5", 5000.5)]
    [InlineData(" 7500 ", 7500)]
    public async Task Save_TutarHemVirgulHemNoktaOndalikliOkunur(string input, decimal expected)
    {
        _entry.Open(CreateCard(), DefaultStatementDate, DefaultDueDate);
        _entry.AmountInput = input;
        _entry.MinimumInput = "0";

        await _entry.SaveCommand.ExecuteAsync(null);

        Assert.Equal(expected, _cardService.LastSavedStatement?.Statement.StatementAmount);
    }

    [Theory]
    [InlineData(CreditCardPaymentStrategy.Minimum, CurrentStatementPaymentMode.Minimum)]
    [InlineData(CreditCardPaymentStrategy.FullStatement, CurrentStatementPaymentMode.Full)]
    [InlineData(CreditCardPaymentStrategy.AskEachStatement, CurrentStatementPaymentMode.Minimum)]
    public async Task Save_YeniEkstrede_OdemeSekliKartVarsayilanindanGelir(
        CreditCardPaymentStrategy strategy, CurrentStatementPaymentMode expected)
    {
        _entry.Open(CreateCard(strategy: strategy), DefaultStatementDate, DefaultDueDate);
        _entry.AmountInput = "10000";
        _entry.MinimumInput = "2000";

        await _entry.SaveCommand.ExecuteAsync(null);

        Assert.Equal(expected, _cardService.LastSavedStatement?.Plan.Mode);
        Assert.Null(_cardService.LastSavedStatement?.Plan.CustomAmount);
    }

    [Theory]
    [InlineData(3000, 3000)]
    [InlineData(30000, 10000)]
    public async Task Save_SabitTutarKartinda_OzelTutarEkstreyleSinirlanir(decimal fixedAmount, decimal expected)
    {
        var card = CreateCard(strategy: CreditCardPaymentStrategy.FixedAmount) with { FixedPaymentAmount = fixedAmount };
        _entry.Open(card, DefaultStatementDate, DefaultDueDate);
        _entry.AmountInput = "10000";
        _entry.MinimumInput = "2000";

        await _entry.SaveCommand.ExecuteAsync(null);

        Assert.Equal(CurrentStatementPaymentMode.Custom, _cardService.LastSavedStatement?.Plan.Mode);
        Assert.Equal(expected, _cardService.LastSavedStatement?.Plan.CustomAmount);
    }

    [Fact]
    public async Task Save_EkstreDuzenlenirken_KimlikVeOdemePlaniKorunur()
    {
        var statement = CreateStatement(18200m, 3640m);
        var card = CreateCard(statement: statement) with
        {
            CurrentStatementPaymentPlan = new CurrentStatementPaymentPlan { Mode = CurrentStatementPaymentMode.Custom, CustomAmount = 5000m }
        };
        _entry.Open(card, DefaultStatementDate, DefaultDueDate);
        _entry.AmountInput = "19.000";

        await _entry.SaveCommand.ExecuteAsync(null);

        var saved = Assert.NotNull(_cardService.LastSavedStatement);
        Assert.Equal(statement.Id, saved.Statement.Id);
        Assert.Equal(19000m, saved.Statement.StatementAmount);
        Assert.Equal(CurrentStatementPaymentMode.Custom, saved.Plan.Mode);
        Assert.Equal(5000m, saved.Plan.CustomAmount);
    }

    [Theory]
    [InlineData("", "2000")]
    [InlineData("10000", "")]
    [InlineData("on bin", "2000")]
    public async Task Save_TutarSayiDegilse_KaydetmezVeFormAcikKalir(string amount, string minimum)
    {
        _entry.Open(CreateCard(), DefaultStatementDate, DefaultDueDate);
        _entry.AmountInput = amount;
        _entry.MinimumInput = minimum;

        await _entry.SaveCommand.ExecuteAsync(null);

        Assert.Null(_cardService.LastSavedStatement);
        Assert.NotNull(_dialog.LastAlertMessage);
        Assert.True(_entry.IsOpen);
        Assert.Equal(0, _savedCallbackCount);
    }

    [Fact]
    public async Task Save_KuralIhlalinde_KuralinMesajiniGosterirVeFormAcikKalir()
    {
        _cardService.SaveException = new InvalidOperationException("Kesilmiş ekstre tutarı ve asgari ödeme geçersiz.");
        _entry.Open(CreateCard(), DefaultStatementDate, DefaultDueDate);
        _entry.AmountInput = "1000";
        _entry.MinimumInput = "2000";

        await _entry.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Kesilmiş ekstre tutarı ve asgari ödeme geçersiz.", _dialog.LastAlertMessage);
        Assert.True(_entry.IsOpen);
        Assert.False(_entry.IsSaving);
        Assert.Equal(0, _savedCallbackCount);
    }

    [Fact]
    public void Cancel_FormuKaydetmedenKapatir()
    {
        _entry.Open(CreateCard(), DefaultStatementDate, DefaultDueDate);
        Assert.True(_entry.IsOpen);

        _entry.CancelCommand.Execute(null);

        Assert.False(_entry.IsOpen);
        Assert.Null(_cardService.LastSavedStatement);
    }

    private static CreditCardStatement CreateStatement(decimal amount, decimal minimum) => new()
    {
        StatementDate = new DateOnly(2026, 9, 15), DueDate = new DateOnly(2026, 9, 25),
        StatementAmount = amount, MinimumPaymentAmount = minimum
    };

    private static CreditCard CreateCard(
        CreditCardStatement? statement = null,
        CreditCardPaymentStrategy strategy = CreditCardPaymentStrategy.AskEachStatement)
    {
        var id = Guid.NewGuid();
        return new CreditCard
        {
            Id = id, Name = "Bonus", Bank = "Garanti BBVA", Limit = 50000m,
            BalanceAsOfDate = new DateOnly(2026, 9, 1), StatementClosingDay = 15, PaymentDueDay = 25,
            PaymentStrategy = strategy,
            CurrentStatement = statement is null ? null : statement with { CreditCardId = id }
        };
    }
}
