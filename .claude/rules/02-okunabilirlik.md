---
paths:
  - "src/**/*.cs"
  - "tests/**/*.cs"
---

# 02 — Okunabilirlik

Bu projenin var olma sebebi bu dosyadır. Birinci Mizan çalışıyordu ama sahibi kodu
okuyamıyordu. Buradaki kurallar "temiz kod" hevesi değil, o problemin doğrudan panzehiridir.

## Boyut sınırları

| Birim | Sınır | Sınıra dayandığında |
|---|---|---|
| Dosya | **200 satır** | Dosyayı değil **sorumluluğu** böl |
| Metot | **40 satır** | Bir adımı adı olan özel metoda çıkar |
| İç içe blok | **3 seviye** | Erken `return` ile düzleştir |
| Yapıcı bağımlılığı | **5 adet** | Sınıf iki iş yapıyordur, ikiye ayır |
| Arayüz metodu | **10 adet** | Portu böl |

> **`partial` ile dosya bölmek bu sınırları aşmanın yolu değildir — yasaktır.**
> Eski projede `CommitmentsViewModel` altı `partial` dosyaya bölünerek 1.344 satıra ulaşmıştı
> ve "350 satır kuralı" kâğıt üzerinde geçerliydi. `partial` yalnız `[ObservableProperty]`
> gibi source generator'ların gerektirdiği yerde kullanılır ve **tek dosya** kalır.

## Bir dosya, bir tip

Bir `.cs` dosyasında bir tane public tip bulunur ve dosyanın adı o tipin adıdır.
İstisna: yalnızca o tipin içinde anlamlı olan küçük `private` yardımcı tipler.

Bir dosyaya "şunlar da buraya yakışır" diye ikinci bir enum koyma. Enum kendi dosyasını hak eder.

## İsimlendirme

- **Kod İngilizce.** Yorum, doküman, kullanıcı metni ve commit gövdesi **Türkçe**.
- İsimler `docs/SOZLUK.md`'den gelir. Sözlükte olmayan yeni bir kavram adlandırıyorsan
  **önce sözlüğe ekle**, sonra kodda kullan.
- **Kısaltma yok.** `calc`, `amt`, `vm`, `svc`, `idx` yazma. `calculator`, `amount` yaz.
  Tek istisna, sözlükte tanımlı ve iş dünyasında zaten kısaltma olanlar.
- Bir isim yalan söylemez. Alan `NextPaymentDate` tutuyorsa adı `StartDate` olamaz —
  eski şemada tam 16 böyle kolon vardı ve her sorguda bedeli ödeniyordu.
- Boolean isimleri soru gibi okunur: `IsPaid`, `HasOpenPlan`, `CanClosePeriod`.

## Yorumlar

Her public tipte Türkçe `<summary>` **zorunludur** (CS1591 derleme hatasıdır). İçeriği:

> **Ne yaptığını değil, neden var olduğunu yaz.**

```csharp
// ❌ Kötü — imzanın tekrarı, hiçbir şey öğretmiyor
/// <summary>Kredi itfasını hesaplar.</summary>

// ✅ İyi — neden var olduğunu, hangi kuralı koruduğunu söylüyor
/// <summary>
/// Bankanın açıkladığı taksit tutarından örtük faiz oranını geri çözer.
/// Kullanıcı sözleşmedeki oranı çoğu zaman bilmiyor; elinde yalnızca
/// aylık taksit ve kalan borç oluyor. Oranı bilmeden erken kapama
/// tutarı hesaplanamadığı için bu sınıf var.
/// </summary>
```

Metot içi yorum, **neden** öyle yapıldığını anlatır. "Bu satır X'i Y yapar" yorumu silinir;
onun yerine satır kendini anlatacak şekilde yeniden yazılır.

Türkiye mevzuatına dayanan bir kural varsa maddesini yaz: `// 6502 s. Kanun md. 37`.

## Sihirli sayı yok

```csharp
// ❌
if (remainingMonths <= 36) { fee = balance * 0.01m; }

// ✅
private const int KonutKredisiKisaVadeEsigiAy = 36;
private const decimal KisaVadeErkenOdemeUcretiOrani = 0.01m;
```

Sabitin adı iş anlamını taşır, değerini değil. `const decimal Yuzde Bir = 0.01m` yazma.

## C# 12 deyimleri

Kullan: file-scoped namespace, primary constructor (DI için), collection expression `[]`,
`is null` / `is not null`, `record` (değer tipi modeller için), hedef tipli `new()`.

Kullanma: `#region` (bir dosyayı bölmek gerekiyorsa dosya zaten çok büyüktür),
`dynamic`, `var` ile belirsiz tip (`var result = Get();` — dönen tip okunamıyorsa yaz).

## Bir kod incelemesi sorusu

Bu dosyayı üç ay sonra ilk kez açan biri, **başka hiçbir dosyaya bakmadan** ne yaptığını
anlayabiliyor mu? Anlayamıyorsa problem onun dikkatinde değil, bu dosyadadır.
