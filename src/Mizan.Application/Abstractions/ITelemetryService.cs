namespace Mizan.Application.Abstractions;

/// <summary>
/// Harici telemetri ve çökme raporlama sağlayıcılarını (Sentry vb.) soyutlayan telemetri portu.
/// Çekirdek katmanların harici SDK'lara bağımlı olmadan olay, invaryant ihlali,
/// istisna ve gezinme izi bildirmesini sağlar (Düğüm T9).
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Önemli bir kullanıcı veya sistem olayını ek özniteliklerle kaydeder.
    /// </summary>
    /// <param name="eventName">Kaydedilecek olayın benzersiz adı.</param>
    /// <param name="properties">Olayla ilişkili ek yapılandırılmış öznitelikler.</param>
    void TrackEvent(string eventName, IReadOnlyDictionary<string, string>? properties = null);

    /// <summary>
    /// Finansal hesaplama veya sistem invaryantı ihlalini özel hata koduyla kaydeder.
    /// </summary>
    /// <param name="invariantCode">İhlal edilen invaryantın kodu (örn. I16, BR-CALENDAR-01).</param>
    /// <param name="description">İhlalin iş dilindeki kısa açıklaması.</param>
    /// <param name="details">İhlal anındaki finansal durum parametreleri.</param>
    void TrackInvariantViolation(
        string invariantCode,
        string description,
        IReadOnlyDictionary<string, string>? details = null);

    /// <summary>
    /// Beklenmeyen bir çalışma zamanı istisnasını bağlam bilgileriyle raporlar.
    /// </summary>
    /// <param name="exception">Yakalanan ve raporlanacak istisna nesnesi.</param>
    /// <param name="context">İstisna anındaki bağlam verileri.</param>
    void CaptureException(Exception exception, IReadOnlyDictionary<string, string>? context = null);

    /// <summary>
    /// Olası bir hata anında incelenmek üzere gezinme veya işlem ekmek kırıntısı bırakır.
    /// </summary>
    /// <param name="message">Gezinme veya operasyon mesajı.</param>
    /// <param name="category">İzin kategorisi (varsayılan: navigation).</param>
    void AddBreadcrumb(string message, string category = "navigation");
}
