namespace Mizan.App.Icons;

/// <summary>
/// Ekranlarda kullanılan ikonların kod noktaları. Tek yerde durur: eski projede
/// ok ve simge karakterleri buton metinlerinin içine gömülüydü ("Bu dönemi kapat →"),
/// bu yüzden ne aranabiliyor ne değiştirilebiliyordu. Burada durunca bir ikon
/// silindiğinde derleme kırılır.
/// </summary>
public static class Icons
{
    public const string Menu = "\ue5d2";
    public const string ChevronRight = "\ue5cc";
    public const string ArrowBack = "\ue5c4";
    public const string Close = "\ue5cd";
    public const string Insights = "\uf092";
    public const string Notifications = "\ue7f5";
    public const string Check = "\ue668";
    public const string Schedule = "\uefd6";
    public const string Settings = "\ue8b8";
    public const string CreditCard = "\ue8a1";
    public const string AccountBalance = "\ue84f";
    public const string Payments = "\uef63";
}
