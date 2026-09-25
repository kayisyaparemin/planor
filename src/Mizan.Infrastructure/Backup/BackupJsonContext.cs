using System.Text.Json.Serialization;
using Mizan.Application.Models;

namespace Mizan.Infrastructure.Backup;

/// <summary>
/// Yedek manifestini ve son yedek durumunu Android üzerinde AOT ve kırpma (trimming)
/// güvenli serileştiren JSON bağlamı.
/// </summary>
[JsonSerializable(typeof(BackupManifest))]
[JsonSerializable(typeof(BackupState))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal sealed partial class BackupJsonContext : JsonSerializerContext;
