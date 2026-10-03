using Mizan.Application.Abstractions;
using Mizan.Application.Models;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.Services;

/// <summary>
/// Finansal Yapı ve Simülatör tür seçicilerinin aday kayıtları (kart, kredi, gelir)
/// okuyup süzmesini sağlayan ortak Presentation yardımcısıdır (M8, S77 V10 notları i).
/// </summary>
public sealed class RecordCandidateResolver
{
    private readonly IPlanReader _planReader;
    private readonly FinancialRecordRowBuilder _rowBuilder;

    /// <summary>
    /// Yardımcıyı plan okuyucu ve satır kurucu ile başlatır.
    /// </summary>
    public RecordCandidateResolver(IPlanReader planReader, FinancialRecordRowBuilder rowBuilder)
    {
        _planReader = planReader ?? throw new ArgumentNullException(nameof(planReader));
        _rowBuilder = rowBuilder ?? throw new ArgumentNullException(nameof(rowBuilder));
    }

    /// <summary>
    /// Belirtilen kayıt türüne ait adayları Finansal Yapı süzgecinden geçirerek döner.
    /// Okuma hatası durumunda null döner.
    /// </summary>
    public async Task<IReadOnlyList<FinancialRecordRow>?> GetCandidatesAsync(FinancialRecordKind kind)
    {
        try
        {
            var rows = _rowBuilder.Build(await _planReader.GetPlanAsync());
            return [.. rows.Incomes.Concat(rows.Cards).Concat(rows.Loans).Where(x => x.Kind == kind)];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RecordCandidateResolver ERROR] {ex}");
            return null;
        }
    }
}
