namespace Mizan.Presentation.Models;

/// <summary>
/// Tür seçici ızgarasındaki bir bölüm: grup başlığı ve altındaki karolar. Bütün gruplar aynı anda görünür;
/// şeritten grup seçmek iki-üç seçenekle ekranın altını boş bırakıyordu (V6f Kapı C, GS29). Grup bir
/// <see cref="Enum"/> olarak taşınır: Finansal Yapı kayıt gruplarını, simülatör (V10c) kendi senaryo
/// gruplarını aynı bölümle sunar; başlığın metnini App'teki çevirici bulur.
/// </summary>
/// <param name="Group">Bölümün grubu.</param>
/// <param name="Options">Bölümün karoları, sırasıyla.</param>
public sealed record EntryTypeSection(Enum Group, IReadOnlyList<EntryTypeOptionItem> Options);
