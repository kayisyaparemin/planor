using Mizan.Infrastructure.Time;

namespace Mizan.Infrastructure.Tests.Time;

public sealed class SystemClockTests
{
    [Fact]
    public void Today_SistemGunuyleUyumlu_BugunkuTarihiDondurur()
    {
        // Hazırla
        var clock = new SystemClock();
        var beklenenBugun = DateOnly.FromDateTime(DateTime.Now);

        // Uygula
        var sonuc = clock.Today;

        // Doğrula
        Assert.Equal(beklenenBugun, sonuc);
    }

    [Fact]
    public void UtcNow_SistemUtcZamaniylaUyumlu_GuncelZamaniDondurur()
    {
        // Hazırla
        var clock = new SystemClock();
        var oncesi = DateTimeOffset.UtcNow;

        // Uygula
        var sonuc = clock.UtcNow;
        var sonrasi = DateTimeOffset.UtcNow;

        // Doğrula
        Assert.InRange(sonuc, oncesi, sonrasi);
    }
}
