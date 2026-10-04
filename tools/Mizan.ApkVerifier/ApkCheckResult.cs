namespace Mizan.ApkVerifier;

/// <summary>Tek bir APK kontrolünün sonucu; doğrulayıcı geçen ve kalan her kontrolü ayrı satırda yazar.</summary>
/// <param name="Name">Kontrolün adı (örn. "Paket kimliği").</param>
/// <param name="Passed">Kontrol geçtiyse <see langword="true"/>.</param>
/// <param name="Detail">Bulunan değer ya da ihlalin açıklaması.</param>
public sealed record ApkCheckResult(string Name, bool Passed, string Detail);
