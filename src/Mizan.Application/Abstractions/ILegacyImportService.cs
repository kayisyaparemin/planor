using Mizan.Application.Models;

namespace Mizan.Application.Abstractions;

/// <summary>
/// Eski uygulamanın yedeğini kullanıcının mevcut profillerinin yanına ekleyen kullanım senaryosu portu.
/// Ekranın tek bir çağrıyla "eski verimi getir" diyebilmesi için vardır: profil adlandırma ve kimlik
/// çakışması kuralları ekrana değil burada durur. <see cref="IBackupService"/> 10 metot sınırında olduğu
/// için içe aktarma o porta eklenmedi (M5).
/// </summary>
public interface ILegacyImportService
{
    /// <summary>
    /// Yedekteki bütün profilleri mevcutların yanına ekler; kimliği zaten var olan profil kopya adıyla gelir.
    /// Dosya eski uygulamanın yedeği değilse kullanıcıya gidecek mesajla <see cref="InvalidOperationException"/>
    /// fırlatır; hiçbir profil eklenmez.
    /// </summary>
    Task<BackupImportResult> ImportAsync(
        Stream source,
        CancellationToken cancellationToken = default);
}
