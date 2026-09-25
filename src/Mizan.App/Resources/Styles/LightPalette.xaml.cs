namespace Mizan.App.Resources.Styles;

/// <summary>
/// Açık temanın renk paleti. Koyu paletle aynı anahtarları taşır; hangisinin
/// birleştirileceğine yalnız App karar verir. Ayrı sınıf olmasının sebebi, tema
/// değiştiğinde sözlüğün koddan yeniden oluşturulabilmesi.
/// </summary>
public partial class LightPalette : ResourceDictionary
{
    /// <summary>
    /// <see cref="LightPalette"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public LightPalette() => InitializeComponent();
}
