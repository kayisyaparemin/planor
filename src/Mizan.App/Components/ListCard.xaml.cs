using System.Collections;
using System.Windows.Input;

namespace Mizan.App.Components;

/// <summary>
/// Sayfalarda en fazla 4 satırlık kompakt listeleri özetlemek, liste toplamını göstermek
/// ve 4 satırdan fazla veri olduğunda kullanıcıyı detay sayfasına yönlendirmek için vardır.
/// </summary>
public partial class ListCard : ContentView
{
    /// <summary>Üstteki küçük kategori etiketini belirler.</summary>
    public static readonly BindableProperty EyebrowTextProperty =
        BindableProperty.Create(nameof(EyebrowText), typeof(string), typeof(ListCard), string.Empty);

    /// <summary>Kategori yanındaki açıklayıcı notu belirler.</summary>
    public static readonly BindableProperty NoteTextProperty =
        BindableProperty.Create(nameof(NoteText), typeof(string), typeof(ListCard), string.Empty, propertyChanged: OnMetaChanged);

    /// <summary>Listelenecek veri kaynağını belirler.</summary>
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ListCard), null);

    /// <summary>Liste satırlarının şablonunu belirler.</summary>
    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ListCard), null);

    /// <summary>Taşma satırı metnini belirler (örn. "+4 daha").</summary>
    public static readonly BindableProperty OverflowTextProperty =
        BindableProperty.Create(nameof(OverflowText), typeof(string), typeof(ListCard), string.Empty, propertyChanged: OnMetaChanged);

    /// <summary>Taşma satırına tıklandığında çalışacak komutu belirler.</summary>
    public static readonly BindableProperty OverflowCommandProperty =
        BindableProperty.Create(nameof(OverflowCommand), typeof(ICommand), typeof(ListCard), null);

    /// <summary>Taşma komutuna iletilecek parametreyi belirler.</summary>
    public static readonly BindableProperty OverflowCommandParameterProperty =
        BindableProperty.Create(nameof(OverflowCommandParameter), typeof(object), typeof(ListCard), null);

    /// <summary>Toplam satırı etiketini belirler (örn. "Toplam").</summary>
    public static readonly BindableProperty TotalTextProperty =
        BindableProperty.Create(nameof(TotalText), typeof(string), typeof(ListCard), string.Empty, propertyChanged: OnMetaChanged);

    /// <summary>Toplam satırı değerini belirler.</summary>
    public static readonly BindableProperty TotalValueProperty =
        BindableProperty.Create(nameof(TotalValue), typeof(string), typeof(ListCard), string.Empty, propertyChanged: OnMetaChanged);

    private static readonly BindablePropertyKey HasNotePropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasNote), typeof(bool), typeof(ListCard), false);

    /// <summary>Not alanının görünür olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasNoteProperty = HasNotePropertyKey.BindableProperty;

    private static readonly BindablePropertyKey HasOverflowPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasOverflow), typeof(bool), typeof(ListCard), false);

    /// <summary>Taşma alanının görünür olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasOverflowProperty = HasOverflowPropertyKey.BindableProperty;

    private static readonly BindablePropertyKey HasTotalPropertyKey =
        BindableProperty.CreateReadOnly(nameof(HasTotal), typeof(bool), typeof(ListCard), false);

    /// <summary>Toplam alanının görünür olup olmadığını belirtir.</summary>
    public static readonly BindableProperty HasTotalProperty = HasTotalPropertyKey.BindableProperty;

    /// <summary>ListCard bileşenini başlatır.</summary>
    public ListCard()
    {
        InitializeComponent();
    }

    /// <summary>Eyebrow metnini alır veya ayarlar.</summary>
    public string EyebrowText
    {
        get => (string)GetValue(EyebrowTextProperty);
        set => SetValue(EyebrowTextProperty, value);
    }

    /// <summary>Not metnini alır veya ayarlar.</summary>
    public string NoteText
    {
        get => (string)GetValue(NoteTextProperty);
        set => SetValue(NoteTextProperty, value);
    }

    /// <summary>Veri kaynağını alır veya ayarlar.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Satır şablonunu alır veya ayarlar.</summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>Taşma metnini alır veya ayarlar.</summary>
    public string OverflowText
    {
        get => (string)GetValue(OverflowTextProperty);
        set => SetValue(OverflowTextProperty, value);
    }

    /// <summary>Taşma komutunu alır veya ayarlar.</summary>
    public ICommand? OverflowCommand
    {
        get => (ICommand?)GetValue(OverflowCommandProperty);
        set => SetValue(OverflowCommandProperty, value);
    }

    /// <summary>Taşma komut parametresini alır veya ayarlar.</summary>
    public object? OverflowCommandParameter
    {
        get => GetValue(OverflowCommandParameterProperty);
        set => SetValue(OverflowCommandParameterProperty, value);
    }

    /// <summary>Toplam etiketini alır veya ayarlar.</summary>
    public string TotalText
    {
        get => (string)GetValue(TotalTextProperty);
        set => SetValue(TotalTextProperty, value);
    }

    /// <summary>Toplam değerini alır veya ayarlar.</summary>
    public string TotalValue
    {
        get => (string)GetValue(TotalValueProperty);
        set => SetValue(TotalValueProperty, value);
    }

    /// <summary>Not alanının görünür olup olmadığını alır.</summary>
    public bool HasNote => (bool)GetValue(HasNoteProperty);

    /// <summary>Taşma satırının görünür olup olmadığını alır.</summary>
    public bool HasOverflow => (bool)GetValue(HasOverflowProperty);

    /// <summary>Toplam satırının görünür olup olmadığını alır.</summary>
    public bool HasTotal => (bool)GetValue(HasTotalProperty);

    private static void OnMetaChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListCard card)
        {
            card.SetValue(HasNotePropertyKey, !string.IsNullOrWhiteSpace(card.NoteText));
            card.SetValue(HasOverflowPropertyKey, !string.IsNullOrWhiteSpace(card.OverflowText));
            card.SetValue(HasTotalPropertyKey, !string.IsNullOrWhiteSpace(card.TotalValue));
        }
    }
}
