namespace Mizan.Presentation.Models;

/// <summary>
/// Gelir formundaki tutarlardan birinin ham satırı: geçerlilik tarihi, tutar ve silinip silinemeyeceği.
/// Tarih ve tutar metni App'teki çeviricilerle kurulur (EK-V6d, kural: ham veri).
/// </summary>
/// <param name="Id">Tutar kaydının kimliği; kayıtlı tutarda korunur.</param>
/// <param name="EffectiveDate">Tutarın yürürlüğe girdiği gün.</param>
/// <param name="Amount">O günden itibaren yatan aylık net tutar.</param>
/// <param name="IsRemovable">Tutar bugün ya da sonra yürürlüğe giriyorsa silinebilir (S67-5).</param>
public sealed record IncomeAmountRow(Guid Id, DateOnly EffectiveDate, decimal Amount, bool IsRemovable);
