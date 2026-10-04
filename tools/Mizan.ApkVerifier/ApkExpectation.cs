namespace Mizan.ApkVerifier;

/// <summary>
/// Bir APK'dan beklenenler: csproj'daki kimlik ve derlemenin hangi bağlamda yapıldığı.
/// CI sürüm üretmez, yalnız tutarlılığı denetler (S80-5).
/// </summary>
/// <param name="Identity">csproj'dan okunan beklenen kimlik.</param>
/// <param name="TagVersion">Yayın etiketinden gelen sürüm (<c>v</c> öneki olmadan); PR derlemesinde <see langword="null"/>.</param>
/// <param name="RequireReleaseSignature">Debug sertifikası reddedilsin mi; yalnız yayın iş akışında <see langword="true"/>.</param>
public sealed record ApkExpectation(ApkIdentity Identity, string? TagVersion, bool RequireReleaseSignature);
