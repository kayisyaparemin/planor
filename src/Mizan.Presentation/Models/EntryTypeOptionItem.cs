namespace Mizan.Presentation.Models;

/// <summary>
/// Tür seçici ızgarasındaki bir karo. Anahtar ve bölüm içindeki sıra taşır: başlık ve alt satır App'in metin
/// kataloğundan anahtarla gelir (kural 03), karonun ızgaradaki yerini sayfa sıradan çıkarır (12 Dönem
/// karosu deseni, GS26-1); görünüm modeli kaç sütun olduğunu bilmez. Aynı karo simülatörde de (V10c) kullanılır.
/// </summary>
/// <param name="Key">Katalogdaki seçeneğin anahtarı.</param>
/// <param name="Index">Karonun bölüm içindeki sırası (0'dan başlar).</param>
public sealed record EntryTypeOptionItem(string Key, int Index);
