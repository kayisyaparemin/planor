# İş Akışı — Bir Taşıma Adımı

Eski projeden (`C:\Users\kayis\Documents\mizan`) bu repoya bir parça taşırken izlenecek
protokol. **Yedi aşama vardır ve hiçbiri atlanamaz.**

Bu protokolün amacı kodu taşımak değil; kodun **sahibi tarafından anlaşılmış olarak** taşınmasını
sağlamaktır. Birinci Mizan'ın problemi eksik koddu değildi, anlaşılmamış koddu.

---

## Aşama 1 — Keşif  *(kod yazılmaz)*

Eski repodaki ilgili dosyaları oku. Şunu raporla:

- Hangi dosyalar, kaç satır
- Bu parça hangi tiplere bağlı, hangi tipler buna bağlı
- `docs/TASIMA-PLANI.md`'de bu adımın gerektirdiği önceki adımlar tamamlanmış mı

Bağımlılığı henüz taşınmamış bir şeye rastladıysan **dur ve söyle.** Sırayı kendi başına değiştirme.

## Aşama 2 — Anlatım  *(DURAKLAMA NOKTASI)*

Kullanıcıya Türkçe anlat:

1. **Bu parça ne işe yarıyor?** İş dilinde, kod terimiyle değil.
   ("Kullanıcı bankadan aldığı taksit tutarını biliyor ama faiz oranını bilmiyor; bu sınıf
   oranı geri çözüyor, çünkü erken kapama tutarı oran olmadan hesaplanamıyor.")
2. **Hangi iş kuralını taşıyor?** Varsa mevzuat maddesi, varsa `docs/INVARYANTLAR.md` kodu.
3. **Eski hâlinde ne sorunluydu?** `docs/v2/02-NEDEN-BU-YAPI.md`'deki düğümlerden birine
   dokunuyor mu; yeni hâlinde ne değişecek.
4. **Yeni hâlinde hangi dosyalar oluşacak?** Yol yol liste.

> **Kullanıcı onaylamadan Aşama 3'e geçilmez.** Bu aşama "bilgilendirme" değil, bir kapıdır.
> Kullanıcı soru sorarsa cevapla ve beklemeye devam et. Onay gelmeden tek satır kod yazma.

## Aşama 3 — Sözleşme

Önce genel API'yi yaz, gövdeleri boş bırak:

```csharp
/// <summary>… neden var olduğu …</summary>
public sealed class LoanAmortizationCalculator
{
    public LoanAmortization Calculate(Loan loan) => throw new NotImplementedException();
}
```

Bu aşamada: tipler, arayüzler, `record`'lar, metot imzaları. İş mantığı **yok.**
Derlenmeli. Burada dur ve sözleşmenin doğru olduğundan emin ol — yanlış bir imza,
yanlış yazılmış yirmi testten daha pahalıdır.

## Aşama 4 — Kırmızı test

Davranışı tarif eden testleri yaz. Çalıştır. **Kırmızı olduklarını göster.**

- Testler `NotImplementedException` ile değil, **beklenen değerle** başarısız olmalı
  (yani gerçekten davranış iddia etmeli).
- Sınır durumları dahil et: sıfır, negatif, dönemin ilk ve son günü, tek taksit kalmış kredi.
- Eski projedeki mevcut testlerden **kopyalama**; onları oku, ama iddiayı yeniden yaz.
  Eski testlerin bir kısmı eski hataları dondurmuş olabilir.

## Aşama 5 — Yeşil

En yalın implementasyonu yaz. Testler geçsin.

Burada "ileride lazım olur" diye bir şey ekleme. `docs/TASIMA-PLANI.md`'de sonraki adımda
gelecek olan şey sonraki adımda gelir.

## Aşama 6 — Kalkan

```bash
dotnet build Mizan.sln
```
```bash
dotnet test Mizan.sln
```

Hepsi geçmeli: **0 hata, 0 uyarı, tüm testler yeşil.** Mimari testler (K1–K8) dahil.
Kapsam eşiği düştüyse test ekle, eşiği düşürme.

Bir mimari test kırmızıya düştüyse: **kuralı esnetme, kodu düzelt.** Kural gerçekten yanlışsa
kullanıcıya söyle ve kural değişikliğini ayrı bir iş olarak ele al.

## Aşama 7 — Kayıt

Sırayla:

1. `docs/TASIMA-PLANI.md` — adımın kutusunu işaretle
2. `docs/DURUM.md` — "Bu adım ne getirdi" başlığı altına 3–6 satır
3. Yeni bir invariant doğduysa `docs/INVARYANTLAR.md` — kod, kural, **koruyan testin tam adı**
4. Yeni bir kavram adlandırdıysan `docs/SOZLUK.md`
5. **Tek commit.** Konvansiyon: İngilizce özet satırı + Türkçe gövde.

```
feat(domain): add loan amortization calculator

Bankanın açıkladığı taksit tutarından örtük faiz oranını geri çözen
hesaplayıcı taşındı (eski: LoanAmortizationCalculator.cs, 376 satır).

Eski hâlinde SimulationCalculator bu sınıfı kendi içinde new'liyordu;
artık zorunlu yapıcı parametresi (kural M1).

Adım: D13 — kredi itfası
```

---

## Adım büyüklüğü

- Bir adım en fazla **~300 satır üretim kodu** getirir.
- Bir adım **tek katmana** dokunur.
- Bir adım **tek iş yeteneği** taşır.

Üçünden biri aşılıyorsa adım büyük demektir: Aşama 1'de dur, bölünme öner, kullanıcıya sor.

## Taşımama hakkı

Eski koddaki her şey taşınmak zorunda değil. Bir parçanın artık gerekmediğini düşünüyorsan
**taşımadan önce söyle.** Özellikle şunlar adaydır: ölü kolonlar, kullanılmayan ayar bayrakları,
eski şema göçleri, `*SourceTests` karşılıkları, kaldırılmış özelliklerin kalıntıları.
