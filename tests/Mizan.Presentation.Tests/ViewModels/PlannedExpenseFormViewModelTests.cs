using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Planlı büyük harcama formu testleri: açılış, alan doğrulamaları, kaydetme ve çıkış onayı (EK-V6e, S65-4).
/// </summary>
public sealed class PlannedExpenseFormViewModelTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);

    private readonly FakeObligationManagementService _obligations = new();
    private readonly FakePlannedLargeExpenseRepository _repository = new();
    private readonly FakeNavigationService _navigation = new();
    private readonly FakeDialogService _dialog = new();
    private readonly PlannedExpenseFormViewModel _viewModel;

    public PlannedExpenseFormViewModelTests()
    {
        var clock = new SabitSaat(Today);
        _viewModel = new PlannedExpenseFormViewModel(_obligations, _repository, clock, _navigation, _dialog);
    }

    [Fact]
    public async Task Load_YeniHarcama_BosAlanlarVeBugununTarihiyleAcar()
    {
        await _viewModel.LoadAsync(null);

        Assert.False(_viewModel.IsEditing);
        Assert.Equal(string.Empty, _viewModel.Name);
        Assert.Equal(string.Empty, _viewModel.AmountInput);
        Assert.Equal(Today, _viewModel.ExactDate);
        Assert.False(_viewModel.HasChanges);
        Assert.True(_viewModel.IsContent);
    }

    [Fact]
    public async Task Load_KayitliHarcama_AlanlariDoldurur()
    {
        var expense = new PlannedLargeExpense
        {
            Name = "Yaz tatili",
            Amount = 30_000m,
            ExactDate = Today.AddDays(20),
            Note = "Otel rezervasyonu"
        };
        _repository.Expenses.Add(expense);

        await _viewModel.LoadAsync(expense.Id);

        Assert.True(_viewModel.IsEditing);
        Assert.Equal("Yaz tatili", _viewModel.Name);
        Assert.Contains("30", _viewModel.AmountInput);
        Assert.Equal(Today.AddDays(20), _viewModel.ExactDate);
        Assert.False(_viewModel.HasChanges);
    }

    [Fact]
    public async Task Load_KayitBulunamayinca_UyarirVeGeriDoner()
    {
        await _viewModel.LoadAsync(Guid.NewGuid());

        Assert.Equal(("Kayıt başarısız", "Düzenlenecek harcama bulunamadı."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_AdBosken_UyarirVeKaydetmez()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.AmountInput = "5000";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Kayıt başarısız", "Harcama adı boş bırakılamaz."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Empty(_obligations.SavedLargeExpenses);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-50")]
    [InlineData("abc")]
    public async Task Save_TutarGecersizken_UyarirVeKaydetmez(string invalidAmount)
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Name = "Tatil";
        _viewModel.AmountInput = invalidAmount;

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Kayıt başarısız", "Tutarı sıfırdan büyük bir sayı olarak gir."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Empty(_obligations.SavedLargeExpenses);
    }

    [Fact]
    public async Task Save_TarihGecmisteyken_UyarirVeKaydetmez()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Name = "Tatil";
        _viewModel.AmountInput = "10000";
        _viewModel.ExactDate = Today.AddDays(-1);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(("Kayıt başarısız", "Harcama tarihi bugünden önce olamaz."), (_dialog.LastAlertTitle, _dialog.LastAlertMessage));
        Assert.Empty(_obligations.SavedLargeExpenses);
    }

    [Fact]
    public async Task Save_GecerliYeniHarcama_KaydederVeGeriDoner()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Name = "Tatil";
        _viewModel.AmountInput = "12500";
        _viewModel.ExactDate = Today.AddDays(5);

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_obligations.SavedLargeExpenses);
        Assert.Equal("Tatil", saved.Name);
        Assert.Equal(12_500m, saved.Amount);
        Assert.Equal(Today.AddDays(5), saved.ExactDate);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Save_Duzenleme_VarOlanKaydinUstuneYazarVeNotunuKorur()
    {
        var expense = new PlannedLargeExpense
        {
            Name = "Eski ad",
            Amount = 10_000m,
            ExactDate = Today.AddDays(10),
            Note = "Özel not"
        };
        _repository.Expenses.Add(expense);
        await _viewModel.LoadAsync(expense.Id);

        _viewModel.Name = "Yeni ad";
        _viewModel.AmountInput = "15000";

        await _viewModel.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(_obligations.SavedLargeExpenses);
        Assert.Equal(expense.Id, saved.Id);
        Assert.Equal("Yeni ad", saved.Name);
        Assert.Equal(15_000m, saved.Amount);
        Assert.Equal("Özel not", saved.Note);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikYoksa_OnaysizGeriDoner()
    {
        await _viewModel.LoadAsync(null);

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(0, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_DegisiklikVarsa_OnaySorar()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Name = "Yeni";
        _dialog.NextConfirmResponse = false;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.Equal("Kaydetmeden çık", _dialog.LastConfirmTitle);
        Assert.False(_navigation.NavigateBackCalled);
    }

    [Fact]
    public async Task Cancel_OnaydaCikSecilince_GeriDoner()
    {
        await _viewModel.LoadAsync(null);
        _viewModel.Name = "Yeni";
        _dialog.NextConfirmResponse = true;

        await _viewModel.CancelCommand.ExecuteAsync(null);

        Assert.Equal(1, _dialog.ConfirmCount);
        Assert.True(_navigation.NavigateBackCalled);
    }
}
