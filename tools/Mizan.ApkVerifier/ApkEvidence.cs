namespace Mizan.ApkVerifier;

/// <summary>
/// Bir APK hakkında Android araçlarından toplanan ham kanıt. Kurallar yalnız bu metinlere bakar;
/// araçları çalıştırmak <c>scripts/verify-apk.ps1</c>'in işidir, böylece kurallar araçsız test edilir.
/// </summary>
/// <param name="Badging"><c>aapt2 dump badging</c> çıktısı.</param>
/// <param name="Certificates"><c>apksigner verify --print-certs</c> çıktısı.</param>
/// <param name="Manifest"><c>aapt2 dump xmltree --file AndroidManifest.xml</c> çıktısı (birleşmiş manifest).</param>
/// <param name="SignatureVerified"><c>apksigner verify</c> sıfır çıkış koduyla döndüyse <see langword="true"/>.</param>
public sealed record ApkEvidence(string Badging, string Certificates, string Manifest, bool SignatureVerified);
