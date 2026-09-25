using Mizan.Infrastructure.Backup;

namespace Mizan.Infrastructure.Tests.Fakes;

/// <summary>
/// Android'in depolama izni yerine geçen sahte. İzin başta verili değilse ve
/// <see cref="IstenirseVerir"/> açıksa, kullanıcının istek ekranında izni verdiğini taklit eder.
/// </summary>
internal sealed class SahteDepolamaIzni : IStorageAccess
{
    public bool HasAccess { get; private set; }

    public bool IstenirseVerir { get; init; }

    public Task<bool> RequestAccessAsync()
    {
        HasAccess |= IstenirseVerir;
        return Task.FromResult(HasAccess);
    }
}
