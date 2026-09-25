namespace Mizan.App.Resources.Styles;

/// <summary>
/// Koyu temanın renk paleti. Açık paletle aynı anahtarları taşır; hangisinin
/// birleştirileceğine yalnız App karar verir. Ayrı sınıf olmasının sebebi, tema
/// değiştiğinde sözlüğün koddan yeniden oluşturulabilmesi.
/// </summary>
public partial class DarkPalette : ResourceDictionary
{
    /// <summary>
    /// <see cref="DarkPalette"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public DarkPalette() => InitializeComponent();
}
