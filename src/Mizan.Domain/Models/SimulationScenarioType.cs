namespace Mizan.Domain.Models;

/// <summary>
/// Kullanıcının nakit akış projeksiyonu üzerinde varsayımsal (hipotetik) olarak deneyebileceği
/// harcama, borçlanma, gelir artışı veya borç kapatma senaryolarının tür sınıflandırması.
/// </summary>
public enum SimulationScenarioType
{
    /// <summary>Peşin ödenen tek seferlik büyük harcama.</summary>
    CashPurchase,

    /// <summary>Kredi kartıyla yapılan tek çekim harcama.</summary>
    CreditCardSinglePayment,

    /// <summary>Kredi kartıyla yapılan taksitli harcama.</summary>
    CreditCardInstallmentPurchase,

    /// <summary>Çekilen anaparanın hesaba girdiği ve taksitlerle geri ödendiği finansman kredisi.</summary>
    FinancingLoan,

    /// <summary>Banka dışı, elden veya senetli taksitli nakit borç.</summary>
    CashDebt,

    /// <summary>Gelecekte belirli bir tarihte yapılacak tek seferlik nakit ödeme.</summary>
    FutureOneTimePayment,

    /// <summary>Belirli bir süre boyunca düzenli tekrar eden periyodik nakit ödeme.</summary>
    RecurringPayment,

    /// <summary>Gelecekte belirli bir tarihte beklenen tek seferlik arızi gelir (ikramiye, prim vb.).</summary>
    FutureIncome,

    /// <summary>Düzenli bir gelir akışının tutarındaki artış veya revizyon (zam, ek gelir vb.).</summary>
    IncomeChange,

    /// <summary>Kredi kartının asgari veya ekstre borcunun tamamı olarak ödenmesi tercihi.</summary>
    CreditCardPaymentMode,

    /// <summary>Mevcut bir banka kredisinin kalan anapara ve yasal indirimlerle tamamen kapatılması.</summary>
    LoanEarlyClosure,

    /// <summary>Mevcut bir banka kredisine kısmi ara ödeme yapılarak vade veya taksitin düşürülmesi.</summary>
    LoanPartialPrepayment
}
