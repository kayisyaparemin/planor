using Mizan.Domain.Models;

namespace Mizan.Presentation.Models;

/// <summary>
/// Kartın ayrı karar verilmeyen ekstreler için varsayılan ödeme şekli ve "her ekstrede sor"
/// seçiliyken hesabın varsaydığı ödeme (EK-V7 S4).
/// </summary>
/// <param name="Strategy">Kartın varsayılan ödeme şekli.</param>
/// <param name="Fallback">Karar verilmemiş ekstrelerde hesabın varsayımı.</param>
public sealed record CardPaymentDefault(CreditCardPaymentStrategy Strategy, ProjectionFallbackStrategy Fallback);
