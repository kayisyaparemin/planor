using SQLite;

namespace Mizan.Infrastructure.Persistence.Entities;

/// <summary>
/// Kullanıcının What-If simülatöründe kaydettiği varsayımsal plan taslağının SQLite tablo varlığı.
/// </summary>
[Table("simulation_drafts")]
internal sealed class SimulationDraftEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string CreatedAt { get; set; } = string.Empty;

    public string UpdatedAt { get; set; } = string.Empty;
}
