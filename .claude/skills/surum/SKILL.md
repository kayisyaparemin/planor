---
name: surum
description: Yayınlanmamış değişikliklerden yeni bir Planör sürümü çıkarır — sürüm numarası, CHANGELOG, main'e push, etiket, yayın koşusunu bitene kadar izleme. /development ve /bug-fix'in son aşaması; düşmüş bir yayını yeniden denemek için tek başına da çağrılır.
disable-model-invocation: true
---

# İş Akışı — Sürüm

`/development` ve `/bug-fix` bu dosyayla biter. Sürüm adımlarının **tek kaynağı** burasıdır; iki
protokol de buraya gönderir, adımları kendi içinde tekrarlamaz.

Sürüm otoritesi csproj'dur (`S80`-5), not `CHANGELOG.md`'den okunur (`S82`). Csproj, CHANGELOG ve
etiket **birlikte** değişir. Yayını GitHub yapar (`release.yml`): önce CI kapısı, sonra imza,
doğrulama ve Release. Telefon yalnız aynı anahtarla imzalı APK ile güncellenir; imza ve secret
işleri kullanıcınındır.

Bu protokolün onay kapısı yoktur: onay, çağıran protokolün Kapı A'sında sürüm notuyla birlikte
alınmıştır. Aşağıda **dur** denen yerlerde durur ve kullanıcıya söyler; başka yerde durmaz.

---

## Aşama 1 — Ön koşul

- Çalışma ağacı temiz (`git status --porcelain` boş). Değilse **dur**: commit'lenmemiş iş sürüme
  girmez.
- `CHANGELOG.md`'de `## [Yayınlanmamış]` altında en az bir `- ` maddesi var. Yoksa yayınlanacak
  bir şey yok: söyle ve protokolü bitir.
- Tek başına çağrıldıysan Kalkan'ı koş (`dotnet build` + `dotnet test`, 0 hata, 0 uyarı). Bir
  protokolün sonundaysan Kalkan orada koştu; yeniden koşma.

## Aşama 2 — main ile eşitle

Sürüm yalnız `origin/main`'e girmiş bir commit'ten çıkar. Ayrı dallarda yazılıp main'e hiç
girmeyen üç düzeltme 0.1.1'e ancak sonradan toplanabildi (`docs/DURUM.md` → Sürüm 0.1.1).

```bash
git fetch origin
```
```bash
git merge-base --is-ancestor origin/main HEAD
```

İkincisi başarısızsa main ilerlemiştir: `origin/main`'i HEAD'e birleştir. Uygulamanın açtığı bir
worktree'deysen `sync_with_base_branch` aracıyla, değilse `git merge origin/main` ile. Çakışmayı
çöz ve Kalkan'ı yeniden koş.

## Aşama 3 — Sürüm numarası

Numara **şimdi**, eşitlemeden sonra belirlenir; protokolün başında değil. Araya başka bir oturum
sürüm sokmuş olabilir.

| Alan | Kural |
|---|---|
| `ApplicationVersion` (versionCode) | Her zaman +1 |
| `ApplicationDisplayVersion` | `[Yayınlanmamış]` altında `### Eklendi` ya da `### Değişti` varsa **minor** (`0.1.1` → `0.2.0`); yalnız `### Düzeltildi` varsa **patch** (`0.1.1` → `0.1.2`) |
| Major (`1.0.0`) | Hiçbir zaman kendiliğinden artmaz; kullanıcının kararıdır |

Etiket boşta olmalı: `git ls-remote --tags origin vX.Y.Z` boş dönmeli. Dönmüyorsa **dur.**

## Aşama 4 — Sürüm commit'i

1. `src/Mizan.App/Mizan.App.csproj` — `ApplicationDisplayVersion` ve `ApplicationVersion`.
2. `CHANGELOG.md` — `[Yayınlanmamış]`'ın maddeleri, alt başlıklarıyla, yeni
   `## [X.Y.Z] - YYYY-AA-GG` bölümüne taşınır (bugünün tarihi); üstte boş bir
   `## [Yayınlanmamış]` kalır. Madde metinlerine dokunma: Kapı A'da onaylandılar.
3. `docs/DURUM.md` — en üstteki girdi bu sürümle gelen işin girdisiyse sonuna
   `Sürüm: X.Y.Z (versionCode N).` satırı; değilse yeni bir `### Sürüm X.Y.Z` girdisi.
4. Notun sürümle eşleştiğini yerelde dene. CI'da düşerse etiket boşa gitmiş olur:

```bash
dotnet test tests/Mizan.Architecture.Tests --filter "FullyQualifiedName~ChangelogTests" -nologo -v q
```

5. Commit:

```
chore(release): X.Y.Z

Sürüm X.Y.Z (versionCode N): <ne getirdi, tek cümle>.
Not CHANGELOG.md'de.
```

## Aşama 5 — Etiket ve push

Etiket sürüm commit'ine **yerelde** konur, sonra main ve etiket tek komutla gider:

```bash
git tag -a vX.Y.Z -m "Planör X.Y.Z"
```
```bash
git push --atomic --follow-tags origin HEAD:main
```

`--follow-tags` pushlanan commit'e bağlı açıklamalı etiketi de götürür; `--atomic` ikisini hep ya
hiç gönderir. İzin listesindeki tek `git push` biçimi budur; başka biçim yazma.

Push reddedilirse (biri araya push etmiş) hiçbir şey gitmemiştir: yerel etiketi sil
(`git tag -d vX.Y.Z`) ve **dur.** Sürüm commit'ini birleştirme çakışmasıyla kurtarmaya çalışma:
csproj ve CHANGELOG'da iki sürüm birden yazılmış olur.

Etiket GitHub'a ulaştığı an `release.yml` başlar. Bu noktadan sonra etiket kendiliğinden
**taşınmaz.**

## Aşama 6 — İzle ve doğrula

```powershell
./scripts/surum-izle.ps1 -Tag vX.Y.Z
```

**Arka planda** çalıştır (zaman aşımı 90 dakika); bittiğinde haber gelir. Tipik süre 13–15
dakikadır (CI kapısı + imza). Beklerken kullanıcıya etiketi ve koşu adresini söyle; yoklama
yapma, `sleep` ile bekleme.

Betik 0 ile bittiyse Release imzalı APK ile yayındadır. Kullanıcıya tek paragraf yaz: sürüm,
ne getirdi, Release adresi. HEAD yerel `main` değilse (worktree dalı) ana çalışma ağacındaki
`main`'in geride kaldığını ve `git pull --ff-only` gerektiğini de söyle. **Protokol burada biter.**

### Koşu düşerse

Betik düşen adımın günlüğünün sonunu yazar. Önce sınıflandır:

| Düşen | Ne yapılır |
|---|---|
| Altyapı: workload ya da NuGet indirmesi, koşucu, ağ, zaman aşımı | `gh run rerun <id> --failed`, sonra `./scripts/surum-izle.ps1 -Tag vX.Y.Z -RunId <id>`. En fazla iki kez; üçüncüde **dur** |
| İmza secret'ı eksik ya da yanlış | **Dur**, hangi secret olduğunu söyle. Secret'ları kullanıcı yazar |
| Derleme, test, kapsam, APK doğrulama | Kodda sorun var. **Dur** ve aşağıdaki iki yolu sun |

Kodda sorun varsa Release oluşmamıştır ama etiket GitHub'dadır. Kullanıcı seçer:

1. **Etiketi kaldır, aynı sürümle tekrar** *(Önerilen — Release oluşmadığı için o sürümü kimse
   görmedi)*. Hata düzeltilir: kırmızı test → yeşil → Kalkan, `/bug-fix` Aşama 4–7 gibi; düzeltme
   kendi `fix(...)` commit'idir, CHANGELOG'a madde eklemez. Etiketi kullanıcı kendi terminalinde
   kaldırır (uzak etiket silmek ajana yasak listesindedir):
   `git push origin --delete vX.Y.Z` ve `git tag -d vX.Y.Z`. Sonra Aşama 5 (etiket düzeltme
   commit'ine konur) ve Aşama 6.
2. **Etiketi bırak, sonraki sürüm.** CHANGELOG'daki X.Y.Z bölümünün başına
   `- Bu sürüm yayınlanmadı; değişiklikleri X.Y.Z+1'de.` maddesi düşülür; düzeltme `/bug-fix`
   ile kendi patch sürümünü çıkarır.

---

## Bu protokolün yapmadıkları

- Secret yazmaz, imza anahtarına dokunmaz, `release.yml`'i değiştirmez.
- Etiketi taşımaz ve uzak etiket silmez.
- Force push yapmaz (izin listesinde yasak).
- Telefona kurmaz: APK'yı Release sayfasından kullanıcı indirir.
