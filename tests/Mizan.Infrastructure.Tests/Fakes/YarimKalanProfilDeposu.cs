using Mizan.Application.Abstractions;
using Mizan.Application.Models;

namespace Mizan.Infrastructure.Tests.Fakes;

/// <summary>
/// Kayıtları gerçek depoya ileten ama <paramref name="patlayanKayit"/>'inci kaydı yazmadan patlayan
/// profil deposu. Geri yüklemede taşımanın yarıda kalmasını (disk doldu, izin kalktı) taklit eder.
/// </summary>
internal sealed class YarimKalanProfilDeposu(IProfileRepository gercek, int patlayanKayit) : IProfileRepository
{
    private int _kayitSayisi;

    public Task<IReadOnlyList<UserProfile>> GetProfilesAsync(CancellationToken cancellationToken = default) =>
        gercek.GetProfilesAsync(cancellationToken);

    public Task SaveProfileAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        _kayitSayisi++;
        if (_kayitSayisi == patlayanKayit)
        {
            throw new IOException("Disk doldu.");
        }

        return gercek.SaveProfileAsync(profile, cancellationToken);
    }

    public Task DeleteProfileAsync(Guid profileId, CancellationToken cancellationToken = default) =>
        gercek.DeleteProfileAsync(profileId, cancellationToken);
}
