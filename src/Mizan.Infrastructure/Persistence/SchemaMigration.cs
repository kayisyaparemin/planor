namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Şemayı bir önceki sürümden <see cref="Version"/>'a taşıyan tek adım. Boş veritabanının kurulumu da
/// eski bir profilin yükseltilmesi de aynı adımlardan geçer; iki ayrı yol olmadığı için yeni kurulum ile
/// yükseltilen profil birbirinden kayamaz (S69). Yayımlanmış bir adım değiştirilmez: telefondaki
/// veritabanları onu zaten çalıştırmıştır, değişiklik yeni bir adım olur.
/// </summary>
/// <param name="Version">Adım çalıştıktan sonra veritabanının <c>user_version</c>'ı.</param>
/// <param name="Commands">Sırayla çalışan SQL komutları.</param>
public sealed record SchemaMigration(int Version, IReadOnlyList<string> Commands);
