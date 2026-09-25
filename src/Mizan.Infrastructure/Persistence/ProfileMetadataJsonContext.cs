using System.Text.Json.Serialization;

namespace Mizan.Infrastructure.Persistence;

/// <summary>
/// Profil meta verilerini Android üzerinde AOT ve kırpma (trimming) güvenli serileştiren JSON bağlamı.
/// </summary>
[JsonSerializable(typeof(ProfileMetadata))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal sealed partial class ProfileMetadataJsonContext : JsonSerializerContext;
