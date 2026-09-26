namespace Mizan.Presentation.Models;

/// <summary>
/// Profil seçim ekranında listelenen tek bir profil kartının görsel sunum verisini taşır.
/// </summary>
public sealed record ProfileCardItem(
    Guid Id,
    string Name,
    string Initial,
    string Detail);
