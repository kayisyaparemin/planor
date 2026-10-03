using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Calculations;
using Mizan.Domain.Models;
using Mizan.Presentation.Models;
using Mizan.Presentation.Services;
using Mizan.Presentation.Tests.Fakes;
using Xunit;
using static Mizan.Presentation.Tests.Services.FinancialStructureData;

namespace Mizan.Presentation.Tests.Services;

public sealed class RecordCandidateResolverTests
{
    private readonly FakePlanReader _reader = new();
    private readonly RecordCandidateResolver _resolver;

    public RecordCandidateResolverTests()
    {
        var builder = new FinancialRecordRowBuilder(new CreditCardStatementCalculator(), new IncomeResolver(), new SabitSaat(Today));
        _resolver = new RecordCandidateResolver(_reader, builder);
    }

    [Fact]
    public async Task GetCandidatesAsync_KartlariSüzer()
    {
        var card = Card("Bonus");
        _reader.Plan = new FinancialPlan { CreditCards = [card] };

        var result = await _resolver.GetCandidatesAsync(FinancialRecordKind.CreditCard);

        Assert.NotNull(result);
        var item = Assert.Single(result);
        Assert.Equal(card.Id, item.Id);
        Assert.Equal("Bonus", item.Name);
    }

    [Fact]
    public async Task GetCandidatesAsync_DepoHatasi_NullDoner()
    {
        var failingReader = new FailingPlanReader();
        var builder = new FinancialRecordRowBuilder(new CreditCardStatementCalculator(), new IncomeResolver(), new SabitSaat(Today));
        var resolver = new RecordCandidateResolver(failingReader, builder);

        var result = await resolver.GetCandidatesAsync(FinancialRecordKind.CreditCard);

        Assert.Null(result);
    }

    private sealed class FailingPlanReader : IPlanReader
    {
        public Task<FinancialPlan> GetPlanAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Disk hatası");

        public Task<ProjectionQueryPlan> GetProjectionPlanAsync(DateOnly asOf, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Disk hatası");
    }
}
