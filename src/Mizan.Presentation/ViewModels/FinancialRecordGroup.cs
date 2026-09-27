using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Finansal Yapı'daki bir grubun (Gelirler, Kartlar, Krediler, Ödemeler) görünen satırları. Liste
/// kartı en fazla dört satır gösterir; fazlası "+N daha" satırıyla grubun kendisinde açılır, çünkü
/// gidilecek bir detay ekranı yoktur (EK-V6, GS21).
/// </summary>
public sealed partial class FinancialRecordGroup : ObservableObject
{
    /// <summary>Grup kapalıyken gösterilen en fazla satır sayısı (GS12).</summary>
    public const int CollapsedRowLimit = 4;

    private IReadOnlyList<FinancialRecordRow> _rows = [];
    private bool _isExpanded;

    [ObservableProperty] private bool hasItems;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOverflow))]
    private int hiddenCount;

    /// <summary>Ekranda görünen satırlar.</summary>
    public ObservableCollection<FinancialRecordRow> Items { get; } = [];

    /// <summary>Gizli satır varsa taşma satırı görünür.</summary>
    public bool HasOverflow => HiddenCount > 0;

    /// <summary>Grubun satırlarını yerleştirir; grup daha önce açıldıysa açık kalır.</summary>
    public void Apply(IReadOnlyList<FinancialRecordRow> rows)
    {
        _rows = rows ?? throw new ArgumentNullException(nameof(rows));
        Refresh();
    }

    /// <summary>Gizli satırları da gösterir.</summary>
    [RelayCommand]
    private void Expand()
    {
        _isExpanded = true;
        Refresh();
    }

    private void Refresh()
    {
        var visible = _isExpanded ? _rows : _rows.Take(CollapsedRowLimit).ToArray();
        Items.Clear();
        foreach (var row in visible)
        {
            Items.Add(row);
        }

        HasItems = Items.Count > 0;
        HiddenCount = _rows.Count - Items.Count;
    }
}
