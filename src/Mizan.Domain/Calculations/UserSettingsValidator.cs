using Mizan.Domain.Models;

namespace Mizan.Domain.Calculations;

/// <summary>
/// Kullanıcı ayarlarının dönem çapası, yaşam gideri ve faiz parametrelerine uygunluğunu doğrulayan saf denetleyici.
/// </summary>
public static class UserSettingsValidator
{
    /// <summary>
    /// Kullanıcı ayarlarının dönem çapası, yaşam gideri ve faiz parametrelerini doğrular.
    /// </summary>
    /// <param name="settings">Doğrulanacak kullanıcı ayarları.</param>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> veya dönem çapası null ise fırlatılır.</exception>
    /// <exception cref="InvalidOperationException">Yaşam gideri negatif ise fırlatılır.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Faiz oranları %0-%100 aralığı dışında ise fırlatılır.</exception>
    public static void Validate(UserSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settings.PeriodAnchor);

        if (settings.PeriodVariableExpenseAllowance < 0m)
        {
            throw new InvalidOperationException("Dönem yaşam gideri negatif olamaz.");
        }

        if (settings.CreditCardCarryInterestRate is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                "Kredi kartı akdi faiz oranı %0 ile %100 arasında olmalıdır.");
        }

        if (settings.DeficitFinancingInterestRate is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                "Finansman açığı faiz oranı %0 ile %100 arasında olmalıdır.");
        }
    }
}
