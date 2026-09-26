using Mizan.Presentation.Navigation;

namespace Mizan.Presentation.Tests.Fakes;

/// <summary>
/// Birim testlerinde sayfa navigasyonunu ve rotaları doğrulayan test dublörü.
/// </summary>
public sealed class FakeNavigationService : INavigationService
{
    public string? LastNavigatedRoute { get; private set; }
    public IDictionary<string, object>? LastParameters { get; private set; }
    public bool NavigateBackCalled { get; private set; }
    public bool PopModalCalled { get; private set; }

    public Task NavigateToAsync(string route, IDictionary<string, object>? parameters = null)
    {
        LastNavigatedRoute = route;
        LastParameters = parameters;
        return Task.CompletedTask;
    }

    public Task NavigateBackAsync()
    {
        NavigateBackCalled = true;
        return Task.CompletedTask;
    }

    public Task PopModalAsync()
    {
        PopModalCalled = true;
        return Task.CompletedTask;
    }
}
