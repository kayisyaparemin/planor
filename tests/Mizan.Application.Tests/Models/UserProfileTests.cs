using Mizan.Application.Models;

namespace Mizan.Application.Tests.Models;

public sealed class UserProfileTests
{
    [Fact]
    public void VarsayilanDegerler_BeklendigiGibiBaslatilir()
    {
        var profile = new UserProfile();

        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal("Profilim", profile.Name);
        Assert.Equal(default, profile.CreatedAt);
        Assert.Null(profile.LastOpenedAt);
    }

    [Fact]
    public void IkiVarsayilanProfil_FarkliIdUretir()
    {
        var first = new UserProfile();
        var second = new UserProfile();

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void WithKopyalama_OzelDegerlerle_DogruAtanir()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);
        var lastOpenedAt = new DateTimeOffset(2026, 9, 1, 10, 30, 0, TimeSpan.Zero);

        var profile = new UserProfile
        {
            Id = id,
            Name = "Ayşe",
            CreatedAt = createdAt,
            LastOpenedAt = lastOpenedAt
        };

        Assert.Equal(id, profile.Id);
        Assert.Equal("Ayşe", profile.Name);
        Assert.Equal(createdAt, profile.CreatedAt);
        Assert.Equal(lastOpenedAt, profile.LastOpenedAt);
    }

    [Fact]
    public void DefaultName_ProfilimDegeridir()
    {
        Assert.Equal("Profilim", UserProfile.DefaultName);
    }

    [Fact]
    public void MaxNameLength_OtuzKarakterdir()
    {
        Assert.Equal(30, UserProfile.MaxNameLength);
    }
}
