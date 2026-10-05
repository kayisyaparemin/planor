using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Mizan.Application.Models;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Açık dönemde planın dediği ile şu anki durumu yan yana sunar (S87): kart başına ödeme, KMH faizi ve yaşam
/// giderinin planlanan ve harcanan tutarı. Gidişat hesabı bunları zaten üretiyordu ama hiçbir ekran göstermiyordu;
/// dönem sonu plandan saptığında kullanıcı sebebini burada görür. Bağımlılığı yoktur: ana sayfa onu kendi
/// yüklediği gidişatla doldurur, ayrı bir okuma yapılmaz.
/// </summary>
public sealed partial class PeriodComparisonViewModel : ObservableObject
{
    /// <summary>Dönemde ödemesi olan kartlar, vade sonra ad sırasıyla.</summary>
    public ObservableCollection<PeriodCardComparisonRow> Cards { get; } = [];

    [ObservableProperty] private bool hasRows;
    [ObservableProperty] private bool hasDeficitInterest;
    [ObservableProperty] private decimal? plannedDeficitInterest; [ObservableProperty] private decimal? currentDeficitInterest;
    [ObservableProperty] private bool isDeficitInterestAbovePlan;
    [ObservableProperty] private decimal? plannedLivingExpense; [ObservableProperty] private decimal? spentLivingExpense;

    /// <summary>Gidişattan tablonun satırlarını ve yaşam giderinin tutarlarını kurar.</summary>
    /// <param name="progress">Açık dönemin gidişatı.</param>
    public void Show(PeriodProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        Cards.Clear();
        foreach (var card in progress.Cards)
        {
            Cards.Add(new PeriodCardComparisonRow(card.Name, card.Planned, card.Current, card.Current > card.Planned));
        }

        // Bakiye girilmediyse gidişatın KMH faizi yoktur; satır planla görünür, şu an uydurulmaz (S87-2).
        PlannedDeficitInterest = progress.PlannedDeficitInterest;
        CurrentDeficitInterest = progress.ProjectedDeficitInterest;
        HasDeficitInterest = progress.PlannedDeficitInterest > 0m || progress.ProjectedDeficitInterest > 0m;
        IsDeficitInterestAbovePlan = progress.ProjectedDeficitInterest > progress.PlannedDeficitInterest;
        HasRows = Cards.Count > 0 || HasDeficitInterest;

        PlannedLivingExpense = progress.PlannedVariableExpenseAllowance;
        SpentLivingExpense = progress.ObservedLivingSpend;
    }

    /// <summary>Tabloyu boşaltır; açık dönem kalmadığında önceki dönemin rakamları ekranda kalmasın diye.</summary>
    public void Clear()
    {
        Cards.Clear();
        HasRows = HasDeficitInterest = IsDeficitInterestAbovePlan = false;
        PlannedDeficitInterest = CurrentDeficitInterest = PlannedLivingExpense = SpentLivingExpense = null;
    }
}
