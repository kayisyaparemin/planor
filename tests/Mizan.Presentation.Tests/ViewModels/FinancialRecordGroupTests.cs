using Mizan.Presentation.Models;
using Mizan.Presentation.ViewModels;
using Xunit;

namespace Mizan.Presentation.Tests.ViewModels;

/// <summary>
/// Finansal Yapı grubunun dört satır sınırını ve taşmanın grubun kendisinde açılmasını doğrular (GS21).
/// </summary>
public sealed class FinancialRecordGroupTests
{
    private readonly FinancialRecordGroup _group = new();

    [Fact]
    public void Apply_DortVeAltindaSatir_HepsiGorunurTasmaYok()
    {
        _group.Apply(Rows(3));

        Assert.Equal(3, _group.Items.Count);
        Assert.True(_group.HasItems);
        Assert.Equal(0, _group.HiddenCount);
        Assert.False(_group.HasOverflow);
    }

    [Fact]
    public void Apply_DorttenFazlaSatir_IlkDortuGorunurKalaniSayilir()
    {
        var rows = Rows(6);

        _group.Apply(rows);

        Assert.Equal(rows.Take(4), _group.Items);
        Assert.Equal(2, _group.HiddenCount);
        Assert.True(_group.HasOverflow);
    }

    [Fact]
    public void Expand_GizliSatirlarGrubunIcindeAcilir()
    {
        var rows = Rows(6);
        _group.Apply(rows);

        _group.ExpandCommand.Execute(null);

        Assert.Equal(rows, _group.Items);
        Assert.Equal(0, _group.HiddenCount);
        Assert.False(_group.HasOverflow);
    }

    [Fact]
    public void Apply_AcilmisGrup_YenidenYuklemedeAcikKalir()
    {
        _group.Apply(Rows(6));
        _group.ExpandCommand.Execute(null);
        var reloaded = Rows(5);

        _group.Apply(reloaded);

        Assert.Equal(reloaded, _group.Items);
        Assert.False(_group.HasOverflow);
    }

    [Fact]
    public void Apply_BosListe_GrupGizlenir()
    {
        _group.Apply(Rows(2));

        _group.Apply([]);

        Assert.Empty(_group.Items);
        Assert.False(_group.HasItems);
        Assert.False(_group.HasOverflow);
    }

    private static FinancialRecordRow[] Rows(int count) =>
        Enumerable.Range(1, count)
            .Select(i => new FinancialRecordRow(Guid.NewGuid(), FinancialRecordKind.PaymentPlan, $"Plan {i}", 100m * i, new DateOnly(2026, 10, i)))
            .ToArray();
}
