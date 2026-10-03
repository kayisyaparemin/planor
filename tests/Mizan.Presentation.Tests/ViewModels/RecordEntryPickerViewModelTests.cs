using Mizan.Application.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Navigation;
using Mizan.Presentation.Tests.Fakes;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Kayıt türü seçici: Finansal Yapı listesinin dört grubu ayrı bölümler olarak sunulur, her karo bölüm
/// içindeki sırasını taşır ve kendi formunu seçicinin yerine açar (EK-V6f, S77-1, S77-2).
/// </summary>
public sealed class RecordEntryPickerViewModelTests
{
    private readonly FakeNavigationService _navigation = new();
    private readonly RecordEntryPickerViewModel _viewModel;

    public RecordEntryPickerViewModelTests()
    {
        _viewModel = new RecordEntryPickerViewModel(_navigation);
    }

    [Fact]
    public void Acilis_DortBolumuListeninGruplariylaSunar()
    {
        EntryTypeSection[] sections = [_viewModel.IncomeSection, _viewModel.CardSection, _viewModel.LoanSection, _viewModel.PaymentSection];

        Assert.Equal<Enum>(
            [RecordEntryGroup.Income, RecordEntryGroup.Card, RecordEntryGroup.Loan, RecordEntryGroup.Payment],
            sections.Select(x => x.Group));
        Assert.Equal(
            ["recurring-income|income", "credit-card", "bank-loan", "cash|recurring|cash-debt|payment-plan"],
            sections.Select(x => string.Join('|', x.Options.Select(o => o.Key))));
    }

    [Fact]
    public void Acilis_KaroBolumIcindekiSirasiniTasir()
    {
        var indexes = _viewModel.PaymentSection.Options.Select(x => x.Index);

        Assert.Equal([0, 1, 2, 3], indexes);
    }

    [Theory]
    [InlineData("recurring-income", Routes.IncomeForm)]
    [InlineData("income", Routes.AdHocIncomeForm)]
    [InlineData("credit-card", Routes.CardForm)]
    [InlineData("bank-loan", Routes.LoanForm)]
    [InlineData("cash", Routes.PlannedExpenseForm)]
    [InlineData("recurring", Routes.PaymentPlanForm)]
    [InlineData("cash-debt", Routes.PaymentPlanForm)]
    [InlineData("payment-plan", Routes.PaymentPlanForm)]
    public async Task SelectOption_FormunuSecicininYerineKimliksizAcar(string key, string route)
    {
        await _viewModel.SelectOptionCommand.ExecuteAsync(new EntryTypeOptionItem(key, 0));

        Assert.Equal("../" + route, _navigation.LastNavigatedRoute);
        Assert.Null(_navigation.LastParameters);
    }

    [Fact]
    public async Task SelectOption_BosSecim_GezinmeYapmaz()
    {
        await _viewModel.SelectOptionCommand.ExecuteAsync(null);

        Assert.Null(_navigation.LastNavigatedRoute);
    }
}
