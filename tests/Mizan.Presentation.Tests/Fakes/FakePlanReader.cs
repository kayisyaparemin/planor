using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Plan okuma portunun sahtesi: verilen planı döner, kaç kez okunduğunu sayar ve istenirse okuma
/// hatası fırlatır. Projeksiyon sorgusu Finansal Yapı'da kullanılmaz.
/// </summary>
internal sealed class FakePlanReader : IPlanReader
{
    public FinancialPlan Plan { get; set; } = new();
    public Exception? ReadException { get; set; }
    public int ReadCount { get; private set; }

    public Task<FinancialPlan> GetPlanAsync(CancellationToken cancellationToken = default)
    {
        ReadCount++;
        return ReadException is null ? Task.FromResult(Plan) : Task.FromException<FinancialPlan>(ReadException);
    }

    public Task<ProjectionQueryPlan> GetProjectionPlanAsync(DateOnly asOf, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}
