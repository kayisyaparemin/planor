using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mizan.Application.Models;
using Mizan.Presentation.Dialogs;
using Mizan.Presentation.Models;

namespace Mizan.Presentation.ViewModels;

/// <summary>
/// Ana sayfanın "Kalan ödemeler" kartının çocuğu: bu dönem daha ne ödeneceğini üç satırda gösterir, fazlasını
/// yerinde açar (S72-9, S75-9) ve satıra dokununca ödemeyi onay sorarak "Ödedim" diye işaretler (S88). Vadesinden
/// önce yapılan ödemenin bunu söyleyecek başka yeri yoktu: hatırlatıcı kartı yalnız vade günü sorar. Kayıt
/// hatırlatıcının "Ödedim"iyle aynıdır ve aynı yoldan yazılır; ana sayfa cevabı oradan duyup yenilenir.
/// </summary>
public sealed partial class RemainingPaymentsViewModel : ObservableObject
{
    /// <summary>Liste kapalıyken görünen en fazla satır (EK-V3 kesme kararı, S75-9).</summary>
    public const int CollapsedRowLimit = 3;

    private const string PaidConfirmMessage = "Ödendi olarak işaretlensin mi? Kalan ödemelerden çıkar, hatırlatması gelmez.";
    private const string PaidText = "Ödedim";
    private const string CancelText = "Vazgeç";
    private const string PaidFailedTitle = "Ödeme işaretlenemedi";
    private const string PaidFailedMessage = "Ödeme kaydedilirken bir sorun oluştu. Tekrar dene.";

    private readonly ReminderCardViewModel _reminders;
    private readonly IDialogService _dialogService;
    private IReadOnlyList<PeriodRemainingPayment> _payments = [];
    private bool _isExpanded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasItems))]
    private int count;

    [ObservableProperty] private decimal total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOverflow))]
    private int hiddenCount;

    /// <summary>Çocuğu, cevabı yazacak hatırlatıcı kartı ve onayı soracak diyalog servisiyle başlatır.</summary>
    public RemainingPaymentsViewModel(ReminderCardViewModel reminders, IDialogService dialogService)
    {
        _reminders = reminders ?? throw new ArgumentNullException(nameof(reminders));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>Kartta görünen satırlar: ilk üçü, "+N daha"ya dokununca hepsi.</summary>
    public ObservableCollection<DashboardRemainingItem> Items { get; } = [];

    /// <summary>Kartta gizli kalan ödeme var mı; taşma satırı yalnız o zaman görünür.</summary>
    public bool HasOverflow => HiddenCount > 0;

    /// <summary>
    /// Kalan ödeme var mı; yoksa kart görünmez (S88-6). Boş kart yalnız "0 ödeme · 0 ₺" diyordu; hatırlatıcı kartı
    /// ve plan / şu an tablosu da boşken kalkar.
    /// </summary>
    public bool HasItems => Count > 0;

    /// <summary>
    /// Gidişatın kalan ödemelerini gösterir. Liste "+N daha" ile açıldıysa açık kalır: "Ödedim"den sonraki yerinde
    /// yenileme kullanıcının açtığı listeyi kapatmasın (S88-4).
    /// </summary>
    public void Show(PeriodProgress progress)
    {
        _payments = progress.RemainingPayments;
        Count = progress.RemainingPayments.Count;
        Total = progress.RemainingTotal;
        ShowItems();
    }

    /// <summary>Listeyi ilk üç satıra döndürür; ana sayfa her açılışta kapalı listeyle başlar.</summary>
    public void Collapse()
    {
        _isExpanded = false;
        ShowItems();
    }

    /// <summary>Açık dönem yokken listeyi boşaltır.</summary>
    public void Clear()
    {
        _payments = [];
        _isExpanded = false;
        Count = 0;
        Total = 0m;
        ShowItems();
    }

    /// <summary>Gizli satırları yerinde açar; açık dönemin ayrıntısı ana sayfanın kendisi (S75-9).</summary>
    [RelayCommand]
    public void Expand()
    {
        _isExpanded = true;
        ShowItems();
    }

    /// <summary>
    /// Dokunulan ödemeyi onay sorarak "Ödedim" diye işaretler (S88-1, -3). Kaydırırken yanlış dokunuş geri alınamayan
    /// bir işaret bırakmasın diye onay sorulur; geri alma yok (S88-5).
    /// </summary>
    [RelayCommand]
    private async Task MarkAsPaidAsync(DashboardRemainingItem? item)
    {
        if (item is null || !await _dialogService.ConfirmAsync(item.Name, PaidConfirmMessage, PaidText, CancelText))
        {
            return;
        }

        try
        {
            await _reminders.RecordPaidAsync(new PaymentDue(item.DueKey, item.Name, item.DueDate, item.Amount));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RemainingPaymentsViewModel ERROR] {ex}");
            // Cevap yazılamadıysa liste olduğu gibi kalır; kullanıcı aynı satıra yeniden dokunabilir.
            await _dialogService.ShowAlertAsync(PaidFailedTitle, PaidFailedMessage);
        }
    }

    private void ShowItems()
    {
        Items.Clear();
        var visible = _isExpanded ? _payments : _payments.Take(CollapsedRowLimit);
        foreach (var payment in visible)
        {
            Items.Add(new DashboardRemainingItem
            {
                DueKey = payment.DueKey, DueDate = payment.DueDate, Name = payment.Name, Amount = payment.Amount
            });
        }
        HiddenCount = _payments.Count - Items.Count;
    }
}
