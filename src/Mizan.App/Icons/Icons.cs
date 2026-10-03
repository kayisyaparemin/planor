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

    /// <summary>T\u00fcr se\u00e7icide d\u00fczenli gelir karosu: her ay yineleyen para (EK-V6f, GS29).</summary>
    public const string Repeat = "\ue040";

    /// <summary>T\u00fcr se\u00e7icide tek seferlik gelir karosu: prim, sat\u0131\u015f gibi bir kerelik para.</summary>
    public const string AutoAwesome = "\ue65f";

    /// <summary>T\u00fcr se\u00e7icide d\u00fczenli \u00f6deme karosu: takvimde her ay ayn\u0131 g\u00fcn.</summary>
    public const string EventAvailable = "\ue614";

    /// <summary>T\u00fcr se\u00e7icide taksitli bor\u00e7 karosu: borcun dilimlere b\u00f6l\u00fcnmesi.</summary>
    public const string PieChart = "\ue6c4";

    /// <summary>T\u00fcr se\u00e7icide \u00f6deme plan\u0131 karosu: aydan aya de\u011fi\u015fen tutarlar.</summary>
    public const string BarChart = "\ue26b";
}
