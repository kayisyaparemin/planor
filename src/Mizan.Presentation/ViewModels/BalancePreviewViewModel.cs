using CommunityToolkit.Mvvm.ComponentModel;
using Mizan.Application.Models;
using Mizan.Presentation.Charts;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// "Bakiye gir" sayfasının önizleme kartı: girilen bakiye kaydedilseydi dönem sonunun ne olacağını, plana göre
/// farkını ve rotasını taşır (EK-V3 sayfa 2, S73-2). Kart ya bütün hâliyle görünür ya hiç görünmez; yarım kalan bir
/// önizleme (rakam eski, grafik yeni) kullanıcıya kaydetmediği bir şeyi gösterirdi.
/// </summary>
public sealed partial class BalancePreviewViewModel : ObservableObject
{
    [ObservableProperty] private bool isShown;
    [ObservableProperty] private decimal? endingBalance;
    [ObservableProperty] private decimal? deviation;
    [ObservableProperty] private bool isBehindPlan;
    [ObservableProperty] private bool isAheadOfPlan;
    [ObservableProperty] private ChartTrend? trend;

    /// <summary>Taslak gidişatın dönem sonunu, plana göre farkını ve rotasını gösterir.</summary>
    /// <param name="preview">Taslak bakiyeyle hesaplanmış, kaydedilmemiş gidişat.</param>
    public void Show(PeriodProgress preview)
    {
        ArgumentNullException.ThrowIfNull(preview);
        EndingBalance = preview.ProjectedEndingBalance;
        Deviation = preview.EndingDeviation;
        IsBehindPlan = Deviation < 0m;
        IsAheadOfPlan = Deviation > 0m;
        Trend = BalancePathTrend.From(preview);
        IsShown = true;
    }

    /// <summary>Kartı gizler ve değerlerini siler.</summary>
    public void Clear()
    {
        IsShown = IsBehindPlan = IsAheadOfPlan = false;
        EndingBalance = Deviation = null;
        Trend = null;
    }
}
