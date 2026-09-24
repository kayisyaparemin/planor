namespace Mizan.Application.Models;

/// <summary>
/// Kullanıcının girdiği dönem kapanış taslağının nihai onay öncesinde türetilen matematiksel
/// bakiyesini, teyit edilen bakiyesini, kasa mutabakat farkını ve plan karnesini sunan önizleme sözleşmesidir.
/// </summary>
public sealed record PeriodSettlementPreview
{
    /// <summary>Hesap hareketlerinden ve fiilî harcamalardan türetilen matematiksel dönem sonu bakiyesi.</summary>
    public required decimal DerivedEndingBalance { get; init; }

    /// <summary>Kullanıcının teyit ettiği (veya türetilene eşitlediği) kesin kapanış bakiyesi.</summary>
    public required decimal ConfirmedEndingBalance { get; init; }

    /// <summary>Teyit edilen bakiye ile türetilen bakiye arasındaki kasa mutabakat farkı (teyit - türetilen).</summary>
    public required decimal ReconciliationAdjustment { get; init; }

    /// <summary>Dondurulan plan ile taslaktaki gerçekleşme arasındaki 12 kategorili karne kıyaslaması.</summary>
    public required PlanActualComparison Comparison { get; init; }
}
