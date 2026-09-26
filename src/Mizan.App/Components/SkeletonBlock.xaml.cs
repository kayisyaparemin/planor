namespace Mizan.App.Components;

/// <summary>
/// Sayfa ve kartlar asenkron veri beklerken spinner kullanmadan yerleşimin zıplamasını
/// önlemek ve nötr bir iskelet geometri sunmak için vardır.
/// </summary>
public partial class SkeletonBlock : ContentView
{
    /// <summary>İskelet bloğunun köşe yuvarlaklığını belirler.</summary>
    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(SkeletonBlock),
            new CornerRadius(8));

    /// <summary>SkeletonBlock bileşenini başlatır.</summary>
    public SkeletonBlock()
    {
        InitializeComponent();
    }

    /// <summary>Köşe yuvarlaklığını alır veya ayarlar.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
