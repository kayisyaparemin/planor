using System.Globalization;
using Mizan.Application.Models;

namespace Mizan.Application.Services;

/// <summary>
/// Profil adının uzunluk, boşluk ve Türkçe harf duyarsız benzersizlik kurallarını
/// denetleyen saf yardımcı sınıf.
/// </summary>
public static class ProfileNameValidator
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>
    /// Profil adını doğrular ve kırpılmış geçerli adı döner; geçersizse hata fırlatır.
    /// </summary>
    public static string Validate(string? name, IEnumerable<UserProfile> existingProfiles, Guid? exceptId = null)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new InvalidOperationException("Profil adı boş olamaz.");
        }

        if (trimmed.Length > UserProfile.MaxNameLength)
        {
            throw new InvalidOperationException(
                $"Profil adı en fazla {UserProfile.MaxNameLength} karakter olabilir.");
        }

        if (existingProfiles.Any(profile =>
                profile.Id != exceptId &&
                AreNamesEqual(profile.Name, trimmed)))
        {
            throw new InvalidOperationException("Bu adla bir profil zaten var.");
        }

        return trimmed;
    }

    /// <summary>
    /// İsim çakışması durumunda "Ad 2", "Ad 3" şeklinde kullanılabilir benzersiz bir aday isim üretir.
    /// </summary>
    public static string GenerateUniqueName(string baseName, IEnumerable<string> takenNames)
    {
        var taken = takenNames.ToArray();
        var candidate = baseName;
        for (var suffix = 2; taken.Any(existing => AreNamesEqual(existing, candidate)); suffix++)
        {
            candidate = $"{baseName} {suffix}";
        }

        return candidate;
    }

    /// <summary>
    /// İki profil adının Türkçe kültür kurallarına göre aynı olup olmadığını karşılaştırır.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Globalization",
        "CA1309:Use ordinal string comparison",
        Justification = "Profil isimleri Türkçe harf kurallarıyla (İ/i, I/ı) karşılaştırılmalıdır.")]
    public static bool AreNamesEqual(string left, string right) =>
        string.Compare(left, right, TurkishCulture, CompareOptions.IgnoreCase) == 0;
}
