namespace Mizan.Application.Models;

/// <summary>
/// Son kapanan dönemlerde bakiyenin plana göre ne kadar değiştiğini özetleyen sonuç.
/// Dönem sonu bakiyeleri bir stoktur ve dönemler arasında toplanamaz; bu yüzden özet,
/// her dönemin kendi açılışına göre net değişimini toplar (S29).
/// </summary>
/// <param name="PlannedNetChange">Dönemlerin nihai planlarındaki net değişimlerin toplamı (kapanış − açılış).</param>
/// <param name="ActualNetChange">Dönemlerde teyit edilen fiilî net değişimlerin toplamı (teyitli kapanış − açılış).</param>
/// <param name="Difference">Fiilî ile planlanan net değişim arasındaki toplam fark.</param>
/// <param name="PeriodCount">Özete giren kapanmış dönem adedi.</param>
public sealed record HistorySummary(
    decimal PlannedNetChange,
    decimal ActualNetChange,
    decimal Difference,
    int PeriodCount);
