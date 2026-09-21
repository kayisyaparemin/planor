using Mizan.Domain.Calculations;
using Mizan.Domain.Models;

namespace Mizan.Domain.Tests.Calculations;

/// <summary>
/// <see cref="CreditCardPaymentPreferenceResolver"/> sınıfının etkin tarihli çözümleme,
/// sıralama, mükerrer karar denetimi ve doğrulama iş kurallarını test eder.
/// </summary>
public sealed class CreditCardPaymentPreferenceResolverTests
{
    private readonly CreditCardPaymentPreferenceResolver _resolver = new();
    private static readonly Guid CardId = Guid.NewGuid();

    [Fact]
    public void Resolve_GecmisBosIse_NullDondurur()
    {
        // Hazırla
        var statementDate = new DateOnly(2026, 8, 25);
        var history = Array.Empty<CreditCardPaymentPreference>();

        // Uygula
        var result = _resolver.Resolve(statementDate, history);

        // Doğrula
        Assert.Null(result);
    }

    [Fact]
    public void Resolve_KesimTarihindenOncekiEnGuncelTercihiSecer()
    {
        // Hazırla
        var history = new[]
        {
            CreatePreference(new DateOnly(2026, 7, 25), CurrentStatementPaymentMode.Minimum),
            CreatePreference(new DateOnly(2026, 8, 25), CurrentStatementPaymentMode.Full),
            CreatePreference(new DateOnly(2026, 9, 25), CurrentStatementPaymentMode.Custom, customAmount: 5_000m)
        };

        // Uygula
        var preferenceForJuly = _resolver.Resolve(new DateOnly(2026, 7, 25), history);
        var preferenceForAugust = _resolver.Resolve(new DateOnly(2026, 8, 26), history);
        var preferenceForSeptember = _resolver.Resolve(new DateOnly(2026, 10, 1), history);
        var preferenceBeforeAny = _resolver.Resolve(new DateOnly(2026, 7, 24), history);

        // Doğrula
        Assert.NotNull(preferenceForJuly);
        Assert.Equal(CurrentStatementPaymentMode.Minimum, preferenceForJuly.Mode);

        Assert.NotNull(preferenceForAugust);
        Assert.Equal(CurrentStatementPaymentMode.Full, preferenceForAugust.Mode);

        Assert.NotNull(preferenceForSeptember);
        Assert.Equal(CurrentStatementPaymentMode.Custom, preferenceForSeptember.Mode);
        Assert.Equal(5_000m, preferenceForSeptember.CustomAmount);

        Assert.Null(preferenceBeforeAny);
    }

    [Fact]
    public void Resolve_AyniGundeBirdenFazlaKayitVarsa_EnSonOlusturulanKazanir()
    {
        // Hazırla
        var date = new DateOnly(2026, 8, 25);
        var first = new CreditCardPaymentPreference
        {
            CreditCardId = CardId,
            Mode = CurrentStatementPaymentMode.Minimum,
            EffectiveFromStatementDate = date,
            CreatedAt = new DateTimeOffset(2026, 8, 25, 10, 0, 0, TimeSpan.Zero)
        };
        var second = new CreditCardPaymentPreference
        {
            CreditCardId = CardId,
            Mode = CurrentStatementPaymentMode.Full,
            EffectiveFromStatementDate = date,
            CreatedAt = new DateTimeOffset(2026, 8, 25, 15, 30, 0, TimeSpan.Zero)
        };
        var history = new[] { first, second };

        // Uygula
        var resolved = _resolver.Resolve(date, history);

        // Doğrula
        Assert.NotNull(resolved);
        Assert.Equal(CurrentStatementPaymentMode.Full, resolved.Mode);
    }

    [Fact]
    public void Resolve_GecmisNullIse_ArgumentNullExceptionFirlatir()
    {
        // Hazırla
        var statementDate = new DateOnly(2026, 8, 25);

        // Uygula & Doğrula
        Assert.Throws<ArgumentNullException>(() => _resolver.Resolve(statementDate, null!));
    }

    [Fact]
    public void Ordered_GecmisiKronolojikOlarakEskidenYeniyeSiralar()
    {
        // Hazırla
        var item1 = new CreditCardPaymentPreference
        {
            CreditCardId = CardId,
            Mode = CurrentStatementPaymentMode.Minimum,
            EffectiveFromStatementDate = new DateOnly(2026, 7, 25),
            CreatedAt = new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero)
        };
        var item2A = new CreditCardPaymentPreference
        {
            CreditCardId = CardId,
            Mode = CurrentStatementPaymentMode.Minimum,
            EffectiveFromStatementDate = new DateOnly(2026, 8, 25),
            CreatedAt = new DateTimeOffset(2026, 8, 25, 9, 0, 0, TimeSpan.Zero)
        };
        var item2B = new CreditCardPaymentPreference
        {
            CreditCardId = CardId,
            Mode = CurrentStatementPaymentMode.Full,
            EffectiveFromStatementDate = new DateOnly(2026, 8, 25),
            CreatedAt = new DateTimeOffset(2026, 8, 25, 18, 0, 0, TimeSpan.Zero)
        };
        var item3 = new CreditCardPaymentPreference
        {
            CreditCardId = CardId,
            Mode = CurrentStatementPaymentMode.Custom,
            CustomAmount = 2_000m,
            EffectiveFromStatementDate = new DateOnly(2026, 9, 25),
            CreatedAt = new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero)
        };

        // Karışık sıralı liste
        var history = new[] { item2B, item3, item1, item2A };

        // Uygula
        var ordered = _resolver.Ordered(history);

        // Doğrula
        Assert.Equal(4, ordered.Count);
        Assert.Same(item1, ordered[0]);
        Assert.Same(item2A, ordered[1]);
        Assert.Same(item2B, ordered[2]);
        Assert.Same(item3, ordered[3]);
    }

    [Fact]
    public void Ordered_GecmisNullIse_ArgumentNullExceptionFirlatir()
    {
        // Uygula & Doğrula
        Assert.Throws<ArgumentNullException>(() => _resolver.Ordered(null!));
    }

    [Theory]
    [InlineData(CurrentStatementPaymentMode.Minimum, null, CurrentStatementPaymentMode.Minimum, null, true)]
    [InlineData(CurrentStatementPaymentMode.Full, null, CurrentStatementPaymentMode.Full, null, true)]
    [InlineData(CurrentStatementPaymentMode.Minimum, null, CurrentStatementPaymentMode.Full, null, false)]
    [InlineData(CurrentStatementPaymentMode.Minimum, 1000.0, CurrentStatementPaymentMode.Minimum, 2000.0, true)]
    [InlineData(CurrentStatementPaymentMode.Custom, 2500.0, CurrentStatementPaymentMode.Custom, 2500.0, true)]
    [InlineData(CurrentStatementPaymentMode.Custom, 2500.0, CurrentStatementPaymentMode.Custom, 3000.0, false)]
    [InlineData(CurrentStatementPaymentMode.Custom, 2500.0, CurrentStatementPaymentMode.Minimum, null, false)]
    public void RepresentsSameDecision_FarkliModVeTutarlarda_DogruSonucUretir(
        CurrentStatementPaymentMode prefMode,
        double? prefAmount,
        CurrentStatementPaymentMode planMode,
        double? planAmount,
        bool expected)
    {
        // Hazırla
        var preference = new CreditCardPaymentPreference
        {
            CreditCardId = CardId,
            Mode = prefMode,
            CustomAmount = prefAmount.HasValue ? (decimal)prefAmount.Value : null,
            EffectiveFromStatementDate = new DateOnly(2026, 8, 25)
        };
        var plan = new CurrentStatementPaymentPlan
        {
            Mode = planMode,
            CustomAmount = planAmount.HasValue ? (decimal)planAmount.Value : null
        };

        // Uygula
        var result = _resolver.RepresentsSameDecision(preference, plan);

        // Doğrula
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RepresentsSameDecision_NullDurumlariniDogruYonetir()
    {
        // Hazırla
        var preference = new CreditCardPaymentPreference
        {
            CreditCardId = CardId,
            Mode = CurrentStatementPaymentMode.Minimum,
            EffectiveFromStatementDate = new DateOnly(2026, 8, 25)
        };
        var plan = new CurrentStatementPaymentPlan
        {
            Mode = CurrentStatementPaymentMode.Minimum
        };

        // Uygula & Doğrula
        Assert.True(_resolver.RepresentsSameDecision(null, null));
        Assert.False(_resolver.RepresentsSameDecision(preference, null));
        Assert.False(_resolver.RepresentsSameDecision(null, plan));
    }

    [Fact]
    public void Validate_GecerliGecmiste_HataFirlatmaz()
    {
        // Hazırla
        var history = new[]
        {
            CreatePreference(new DateOnly(2026, 8, 25), CurrentStatementPaymentMode.Minimum),
            CreatePreference(new DateOnly(2026, 9, 25), CurrentStatementPaymentMode.Custom, customAmount: 3_000m),
            CreatePreference(new DateOnly(2026, 10, 25), CurrentStatementPaymentMode.Full)
        };

        // Uygula & Doğrula (hata fırlatmamalı)
        _resolver.Validate(history);
    }

    [Fact]
    public void Validate_GecersizModIceriyorsa_InvalidOperationExceptionFirlatir()
    {
        // Hazırla
        var history = new[]
        {
            CreatePreference(new DateOnly(2026, 8, 25), (CurrentStatementPaymentMode)999)
        };

        // Uygula & Doğrula
        var ex = Assert.Throws<InvalidOperationException>(() => _resolver.Validate(history));
        Assert.Contains("geçersiz ödeme şekli", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TarihTanimsizIse_InvalidOperationExceptionFirlatir()
    {
        // Hazırla
        var history = new[]
        {
            CreatePreference(default, CurrentStatementPaymentMode.Minimum)
        };

        // Uygula & Doğrula
        var ex = Assert.Throws<InvalidOperationException>(() => _resolver.Validate(history));
        Assert.Contains("geçerli bir kesim tarihi", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0.0)]
    [InlineData(-150.0)]
    public void Validate_OzelTutarModundaTutarPozitifDegilse_InvalidOperationExceptionFirlatir(double? invalidAmount)
    {
        // Hazırla
        var history = new[]
        {
            CreatePreference(
                new DateOnly(2026, 8, 25),
                CurrentStatementPaymentMode.Custom,
                invalidAmount.HasValue ? (decimal)invalidAmount.Value : null)
        };

        // Uygula & Doğrula
        var ex = Assert.Throws<InvalidOperationException>(() => _resolver.Validate(history));
        Assert.Contains("sıfırdan büyük bir tutar", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_GecmisNullIse_ArgumentNullExceptionFirlatir()
    {
        // Uygula & Doğrula
        Assert.Throws<ArgumentNullException>(() => _resolver.Validate(null!));
    }

    private static CreditCardPaymentPreference CreatePreference(
        DateOnly effectiveFrom,
        CurrentStatementPaymentMode mode,
        decimal? customAmount = null) => new()
    {
        CreditCardId = CardId,
        Mode = mode,
        CustomAmount = customAmount,
        EffectiveFromStatementDate = effectiveFrom,
        CreatedAt = new DateTimeOffset(
            effectiveFrom == default ? 2026 : effectiveFrom.Year,
            effectiveFrom == default ? 1 : effectiveFrom.Month,
            effectiveFrom == default ? 1 : effectiveFrom.Day,
            12, 0, 0, TimeSpan.Zero)
    };
}
