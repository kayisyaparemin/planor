<#
.SYNOPSIS
    Mizan'ı Android emülatöründe derleyip kurar, açar ve emülatörü açık bırakır.

.DESCRIPTION
    Ekran adımlarının (tasarim-adimi.md Aşama 9) tek emülatör komutu. Çalışan bir emülatör
    yoksa verilen AVD'yi ayrı bir pencerede başlatır ve açılmasını bekler; sonra uygulamayı
    Debug olarak derleyip kurar ve ön planda başlatır.

    Kurulumdan önce profiller bilgisayara yedeklenir. .NET'in kurulum görevi cihazdaki sürüm
    uyumsuzsa (örneğin başka bir ortamda imzalanmışsa) paketi sessizce kaldırıp yeniden kurar
    ve uygulamanın bütün verisi silinir; bu 2026-09-27'de V7 Kapı C'de oldu. Betik kurulumun
    sıfırdan olduğunu ilk kurulum zamanından anlar ve profilleri yedekten geri yükler.

    Betik gezinmez, tıklamaz, ekran görüntüsü almaz, tema değiştirmez. Ekrana bakmak
    kullanıcının işidir.

.PARAMETER Avd
    Çalışan emülatör yoksa başlatılacak AVD. Varsayılan: "mizan_emulator"

.PARAMETER BootTimeoutSeconds
    Emülatörün açılması için beklenecek en uzun süre (saniye). Varsayılan: 300

.PARAMETER YedekKlasoru
    Otomatik profil yedeklerinin yazıldığı klasör. Repo dışında durur; içinde kişisel veri var.
    Varsayılan: Belgeler\planor-emulator-yedek\emulator\otomatik

.PARAMETER YedekSayisi
    Saklanacak en yeni otomatik yedek sayısı; eskileri silinir. Varsayılan: 10

.PARAMETER GeriYukle
    Verilirse derleme ve kurulum yapılmaz: bu tar dosyasındaki profiller emülatöre geri
    yüklenir ve uygulama açılır. Yalnız files/profiles altı çıkarılır.
#>
[CmdletBinding()]
param (
    [string]$Avd = "mizan_emulator",
    [int]$BootTimeoutSeconds = 300,
    [string]$YedekKlasoru = (Join-Path ([Environment]::GetFolderPath("MyDocuments")) "planor-emulator-yedek\emulator\otomatik"),
    [int]$YedekSayisi = 10,
    [string]$GeriYukle
)

# adb stderr'e bilgi satırı yazar; "Stop" bunları hata sayıp betiği keserdi.
$ErrorActionPreference = "Continue"

$packageId = "com.mizan.app"
$project = Join-Path $PSScriptRoot "..\src\Mizan.App\Mizan.App.csproj"
$windowsTar = Join-Path $env:SystemRoot "System32\tar.exe"
$remoteTar = "/data/local/tmp/planor-geri.tar"

function Stop-WithError([string]$message) {
    Write-Host "HATA: $message" -ForegroundColor Red
    exit 1
}

function Get-RunningEmulator {
    $line = & adb devices 2>$null | Where-Object { $_ -match '^emulator-\d+\s+device$' } | Select-Object -First 1
    if ($null -eq $line) { return $null }
    return ($line -split '\s+')[0]
}

# Paket kurulu değilse $null. Değer değişmişse paket kaldırılıp yeniden kurulmuştur.
function Get-FirstInstallTime([string]$serial) {
    $line = & adb -s $serial shell dumpsys package $packageId 2>$null | Where-Object { $_ -match 'firstInstallTime=' } | Select-Object -First 1
    if ($null -eq $line) { return $null }
    return ($line -split '=', 2)[1].Trim()
}

function Get-ProfileCount([string]$archive) {
    return @(& $windowsTar -tf $archive 2>$null | Where-Object { $_ -match 'files/profiles/[^/]+/mizan\.db3$' }).Count
}

# Uygulama durdurulup yedeklenir; SQLite dosyası ile günlüğü (-wal) aynı andan kalır.
function Backup-Profiles([string]$serial) {
    $profiles = & adb -s $serial shell run-as $packageId ls files/profiles 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $profiles) { return $null }

    $null = New-Item -ItemType Directory -Force $YedekKlasoru
    $archive = Join-Path $YedekKlasoru ("profiller-{0:yyyyMMdd-HHmmss}.tar" -f (Get-Date))
    & adb -s $serial shell am force-stop $packageId 2>&1 | Out-Null
    # PowerShell 5.1 borusu ikili veriyi metne çevirip bozar; yönlendirmeyi cmd yapar.
    & cmd.exe /c "adb -s $serial exec-out run-as $packageId tar cf - files/profiles > `"$archive`""
    if ((Get-ProfileCount $archive) -eq 0) {
        Remove-Item $archive -ErrorAction SilentlyContinue
        Stop-WithError "Profiller yedeklenemedi; kuruluma geçilmedi."
    }

    Get-ChildItem $YedekKlasoru -Filter "profiller-*.tar" | Sort-Object Name -Descending |
        Select-Object -Skip $YedekSayisi | Remove-Item
    return $archive
}

# Eski günlük dosyaları geri yüklenen veritabanına uygulanmasın diye önce silinir.
function Restore-Profiles([string]$serial, [string]$archive) {
    & adb -s $serial shell am force-stop $packageId 2>&1 | Out-Null
    & adb -s $serial push $archive $remoteTar 2>&1 | Out-Null
    & adb -s $serial shell chmod 644 $remoteTar 2>&1 | Out-Null
    & adb -s $serial shell "run-as $packageId sh -c 'mkdir -p files/profiles && rm -f files/profiles/*/mizan.db3-wal files/profiles/*/mizan.db3-shm && tar xf $remoteTar files/profiles'"
    $extracted = $LASTEXITCODE
    & adb -s $serial shell rm -f $remoteTar 2>&1 | Out-Null
    if ($extracted -ne 0) {
        Stop-WithError "Profiller geri yüklenemedi. Yedek yerinde duruyor: $archive"
    }
}

function Start-App([string]$serial) {
    & adb -s $serial shell am force-stop $packageId 2>&1 | Out-Null
    & adb -s $serial shell monkey -p $packageId -c android.intent.category.LAUNCHER 1 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Stop-WithError "Uygulama başlatılamadı."
    }
}

foreach ($tool in "adb", "emulator", "dotnet", $windowsTar) {
    if ($null -eq (Get-Command $tool -ErrorAction SilentlyContinue)) {
        Stop-WithError "'$tool' bulunamadı."
    }
}

# 1. Emülatörü bul ya da başlat
$serial = Get-RunningEmulator
if ($null -eq $serial) {
    Write-Host "Emülatör başlatılıyor: $Avd" -ForegroundColor Cyan
    $emuPath = (Get-Command emulator).Source
    $null = Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{ CommandLine = "`"$emuPath`" -avd $Avd" }
}

# 2. Açılışı bekle
$deadline = (Get-Date).AddSeconds($BootTimeoutSeconds)
$booted = $false
while ((Get-Date) -lt $deadline) {
    if ($null -eq $serial) { $serial = Get-RunningEmulator }
    if ($null -ne $serial) {
        $bootCompleted = & adb -s $serial shell getprop sys.boot_completed 2>$null
        if ("$bootCompleted".Trim() -eq "1") { $booted = $true; break }
    }
    Start-Sleep -Seconds 2
}
if (-not $booted) {
    Stop-WithError "Emülatör $BootTimeoutSeconds sn içinde açılmadı."
}

# Elle geri yükleme: derleme ve kurulum yok
if ($GeriYukle) {
    if (-not (Test-Path $GeriYukle) -or (Get-ProfileCount $GeriYukle) -eq 0) {
        Stop-WithError "Geri yüklenecek profil yok: $GeriYukle"
    }
    Restore-Profiles $serial $GeriYukle
    Start-App $serial
    Write-Host "$(Get-ProfileCount $GeriYukle) profil geri yüklendi, Mizan açık ($serial)." -ForegroundColor Green
    exit 0
}

# 3. Profilleri yedekle
$installedAt = Get-FirstInstallTime $serial
$backup = if ($null -ne $installedAt) { Backup-Profiles $serial } else { $null }
if ($backup) {
    Write-Host "Profiller yedeklendi: $backup" -ForegroundColor Cyan
}

# 4. Derle ve kur
Write-Host "Derleniyor ve kuruluyor ($serial)..." -ForegroundColor Cyan
& dotnet build $project -f net8.0-android -c Debug -t:Install "-p:AdbTarget=-s $serial" -nologo -v q
if ($LASTEXITCODE -ne 0) {
    Stop-WithError "Derleme ya da kurulum başarısız."
}

# 5. Kurulum sıfırdan olduysa profilleri geri yükle
if ($null -ne $installedAt -and (Get-FirstInstallTime $serial) -ne $installedAt) {
    Write-Host "UYARI: Uygulama sıfırdan kuruldu, cihazdaki veri silindi." -ForegroundColor Yellow
    if ($backup) {
        Restore-Profiles $serial $backup
        Write-Host "UYARI: $(Get-ProfileCount $backup) profil yedekten geri yüklendi: $backup" -ForegroundColor Yellow
    }
}

# 6. Güncel APK'yı ön planda başlat
Start-App $serial
Write-Host "Mizan emülatörde açık ($serial)." -ForegroundColor Green
exit 0
