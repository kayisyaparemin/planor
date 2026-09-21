namespace Mizan.Domain.Models;

/// <summary>
/// Belirli bir takvim gününde krediyi tamamen kapatmanın toplam maliyet dökümünü,
/// anapara, gün işleyen faiz, yasal erken kapama ücreti ve elde edilen faiz tasarrufunu temsil eder.
/// Erken kapama tavsiye motoru ve kullanıcı erken kapama simülasyonları için temel sonuç nesnesidir.
/// </summary>
public sealed record LoanPayoffQuote
{
    /// <summary>Kapatma işleminin yapılacağı hedef takvim günü.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Kapatma gününe kadar normal takviminde ödenmiş sayılan taksit adedi.</summary>
    public int InstallmentsPaidBefore { get; init; }

    /// <summary>Kapatma anında kalan net anapara borcu.</summary>
    public decimal Principal { get; init; }

    /// <summary>Son taksitten kapatma gününe kadar işleyen gün faizi.</summary>
    public decimal AccruedInterest { get; init; }

    /// <summary>6502 sayılı Kanun md. 37 uyarınca alınan yasal erken kapama komisyonu.</summary>
    public decimal Fee { get; init; }

    /// <summary>Erken kapatma sayesinde ödenmekten kurtulunan taksit sayısı.</summary>
    public int InstallmentsRemoved { get; init; }

    /// <summary>Ödenmeyecek taksitlerin nominal toplam tutarı.</summary>
    public decimal RemovedInstallmentTotal { get; init; }

    /// <summary>Kapatma için o gün ödenmesi gereken toplam net nakit çıkışı.</summary>
    public decimal Amount => Principal + AccruedInterest + Fee;

    /// <summary>Ödenmekten kurtulunan taksitler ile kapatma bedeli arasındaki net faiz kazancı.</summary>
    public decimal InterestSaving => RemovedInstallmentTotal - Amount;

    /// <summary>Kapatılacak herhangi bir taksit kalıp kalmadığı.</summary>
    public bool HasAnythingToClose => InstallmentsRemoved > 0;
}
