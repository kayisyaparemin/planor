using CommunityToolkit.Mvvm.ComponentModel;
using Mizan.Application.Models;
using Mizan.Domain.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// Simülatör listesinde bir denemenin satırı (EK-V10 S3). Aç/kapa anahtarı satırdaki <see cref="IsEnabled"/>'a
/// iki yönlü bağlıdır: kullanıcı anahtara dokununca satır değişir, görünüm modeli bunu duyup çalışma listesine
/// yazar. Sorunlu denemenin anahtarı kilitlidir (S76-5): hesaba giremeyen bir şeyi açmak bir şey değiştirmez.
/// Tür ve tarih ham değerdir; satırın bağlam metnini converter yazar.
/// </summary>
public sealed partial class SimulationConditionRow : ObservableObject
{
    [ObservableProperty] private bool isEnabled;

    /// <summary>Satırı çalışma listesindeki denemeden kurar.</summary>
    public SimulationConditionRow(SimulationWorkingCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        Id = condition.Request.ScenarioId;
        Name = condition.Request.Name;
        Type = condition.Request.Type;
        Amount = condition.Request.Amount;
        Date = condition.Request.StartDate;
        Issue = condition.Issue;
        isEnabled = condition.IsEnabled;
    }

    /// <summary>Denemenin kimliği; düzenleme ve silme bununla bulunur.</summary>
    public Guid Id { get; }

    /// <summary>Kullanıcının verdiği ad.</summary>
    public string Name { get; }

    /// <summary>Denemenin türü; satırın tür adını converter yazar.</summary>
    public SimulationScenarioType Type { get; }

    /// <summary>Denemenin tutarı.</summary>
    public decimal Amount { get; }

    /// <summary>Denemenin tarihi.</summary>
    public DateOnly Date { get; }

    /// <summary>Hesaba girmesini engelleyen sorun; yoksa <see cref="SimulationConditionIssue.None"/>.</summary>
    public SimulationConditionIssue Issue { get; }

    /// <summary>Anahtar kullanılabilir mi: sorunlu denemede kilitli.</summary>
    public bool CanToggle => Issue == SimulationConditionIssue.None;

    /// <summary>Satırda sorun işareti görünür mü.</summary>
    public bool HasIssue => !CanToggle;
}
