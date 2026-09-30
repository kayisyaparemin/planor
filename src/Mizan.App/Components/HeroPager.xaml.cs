using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Mizan.App.Components;

/// <summary>
/// Kaydırılan hero kartı: tek kart yerinde iki soru iki sayfada durur ve göz aynı anda tek grafik görür (GS22).
/// Sayfalar üst üste ölçülür, görünmeyen saydamdır; böylece kaydırınca kartın boyu değişmez. Geçiş
/// animasyonsuzdur (TASARIM-SISTEMI § Hareket) ve noktalara dokunarak da yapılır, çünkü kaydırmayı bilmeyen ya
/// da ekran okuyucu kullanan kullanıcı da ikinci sayfaya ulaşmalı (GS24).
/// </summary>
[ContentProperty(nameof(Pages))]
public partial class HeroPager : ContentView
{
    /// <summary>Görünen sayfanın sırasını belirler (0 ilk sayfa); iki yönlü bağlanır.</summary>
    public static readonly BindableProperty PositionProperty =
        BindableProperty.Create(nameof(Position), typeof(int), typeof(HeroPager), 0, BindingMode.TwoWay, propertyChanged: OnPositionChanged);

    /// <summary>HeroPager bileşenini başlatır.</summary>
    public HeroPager()
    {
        InitializeComponent();
        Pages.CollectionChanged += OnPagesChanged;
        PageHost.HandlerChanged += OnPageHostHandlerChanged;
        ShowCurrentPage();
    }

    /// <summary>Kartın sayfaları; XAML'de bileşenin içine doğrudan yazılır.</summary>
    public ObservableCollection<HeroPage> Pages { get; } = [];

    /// <summary>Görünen sayfanın sırasını alır veya ayarlar.</summary>
    public int Position
    {
        get => (int)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    private static void OnPositionChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((HeroPager)bindable).ShowCurrentPage();

    private void OnPagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        PageHost.Children.Clear();
        foreach (var page in Pages)
        {
            PageHost.Children.Add(page);
        }
        Dots.IsVisible = Pages.Count > 1;
        ShowCurrentPage();
    }

    private void OnPageHostHandlerChanged(object? sender, EventArgs e)
    {
        // Kart dikey kayan sayfanın içinde; yatay kaydırmayı sayfanın kaydırmasından platform dinleyicisi ayırır.
        if (PageHost.Handler?.PlatformView is Android.Views.View view && view.Context is { } context)
        {
            view.SetOnTouchListener(new Platforms.Android.HeroPagerSwipeListener(context, ShowAdjacentPage));
        }
    }

    private void ShowCurrentPage()
    {
        var current = Math.Clamp(Position, 0, Math.Max(0, Pages.Count - 1));
        for (var index = 0; index < Pages.Count; index++)
        {
            var isCurrent = index == current;
            Pages[index].Opacity = isCurrent ? 1 : 0;
            Pages[index].InputTransparent = !isCurrent;
            AutomationProperties.SetIsInAccessibleTree(Pages[index], isCurrent);
        }
        FirstActive.IsVisible = current == 0;
        FirstIdle.IsVisible = current != 0;
        SecondActive.IsVisible = current == 1;
        SecondIdle.IsVisible = current != 1;
    }

    private void ShowAdjacentPage(int step) => Position = Math.Clamp(Position + step, 0, Math.Max(0, Pages.Count - 1));

    private void OnFirstDotTapped(object? sender, TappedEventArgs e) => Position = 0;

    private void OnSecondDotTapped(object? sender, TappedEventArgs e) => Position = 1;
}
