namespace Mizan.ApkVerifier;

/// <summary>
/// APK'nın kimliği: Android'in uygulamayı tanıdığı ve kullanıcıya gösterdiği dört değer.
/// Yayın bağlamından (etiket, imza zorunluluğu) ayrı durur, çünkü kimliğin tek otoritesi
/// <c>Mizan.App.csproj</c>'dur (S80-5); bağlam ise iş akışından gelir.
/// </summary>
/// <param name="PackageId">Paket kimliği (<c>ApplicationId</c>).</param>
/// <param name="Label">Uygulama etiketi (<c>ApplicationTitle</c>).</param>
/// <param name="VersionCode">versionCode (<c>ApplicationVersion</c>).</param>
/// <param name="VersionName">versionName (<c>ApplicationDisplayVersion</c>).</param>
public sealed record ApkIdentity(string PackageId, string Label, string VersionCode, string VersionName);
