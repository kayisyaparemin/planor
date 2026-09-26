using Mizan.Presentation.Models;
using Xunit;

namespace Mizan.Presentation.Tests;

/// <summary>
/// ScreenState modelinin durum değerlerini ve sunum katmanı sözleşmesini denetleyen birim testleri.
/// </summary>
public sealed class ScreenStateTests
{
    [Fact]
    public void ScreenState_TumDurumlari_Tasar()
    {
        Assert.True(Enum.IsDefined(typeof(ScreenState), ScreenState.Loading));
        Assert.True(Enum.IsDefined(typeof(ScreenState), ScreenState.Content));
        Assert.True(Enum.IsDefined(typeof(ScreenState), ScreenState.Empty));
        Assert.True(Enum.IsDefined(typeof(ScreenState), ScreenState.Error));
    }

    [Fact]
    public void ScreenState_DegerSayisi_Dorttur()
    {
        var values = Enum.GetValues<ScreenState>();
        Assert.Equal(4, values.Length);
    }
}
