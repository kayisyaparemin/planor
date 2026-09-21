<#
.SYNOPSIS
    Antigravity'nin terminal komutlarini her seferinde sormasini keser.

.DESCRIPTION
    Antigravity izinleri iki katmanda tutar:

      1) ~/.gemini/config/config.json          -> kullanici ayarlari (tum projeler)
      2) ~/.gemini/config/projects/<id>.json   -> proje ayarlari (yalniz o proje)

    "Her zaman izin ver" dedigin komutlar globalPermissionGrants.allow icine
    "command(<komutun tamami>)" olarak yazilir. Onay, binary + alt komut onekiyle
    genellestirilir: "dotnet build" onaylanirsa sonraki "dotnet build ..." sormaz.
    Ama komutta $degisken, $(...), ";", "&&" gibi kabuk yapilari varsa onek esleme
    kapanir ve komutun tamami birebir eslesmedikce her seferinde tekrar sorulur.
    Uzun PowerShell bloklariyla calisiyorsan her komut yeni sayilir; sebebi budur.

    Bu betik iki sey yapar:
      - Proje icin izin on ayarini TURBO'ya ceker (o projede hic sormaz).
      - Genel ayara onek eslemeli bir izin listesi yazar (diger projelere de yarar).

.PARAMETER Proje
    Ayarin uygulanacagi Antigravity proje adi. Varsayilan: mizan-v2

.PARAMETER Genel
    TURBO on ayarini yalniz projeye degil, tum projelere uygular.

.PARAMETER Zorla
    Antigravity acikken de yazar. Onerilmez: uygulama kapanirken ayarlari
    bellekteki haliyle geri yazip degisikligi silebilir.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts/antigravity-izinleri.ps1
#>
[CmdletBinding()]
param(
    [string]$Proje = "mizan-v2",
    [switch]$Genel,
    [switch]$Zorla
)

$ErrorActionPreference = "Stop"

$kokDizin   = Join-Path $env:USERPROFILE ".gemini\config"
$genelDosya = Join-Path $kokDizin "config.json"
$projeDizin = Join-Path $kokDizin "projects"

if (-not (Test-Path $genelDosya)) {
    throw "Antigravity ayar dosyasi bulunamadi: $genelDosya"
}

if (-not $Zorla) {
    if (Get-Process -Name "Antigravity" -ErrorAction SilentlyContinue) {
        throw "Antigravity calisiyor. Once tamamen kapat, sonra bu betigi tekrar calistir."
    }
}

# Onek eslemeli izin listesi. "git log" girdisi "git log --oneline --stat" gibi
# tum varyantlari kapsar. "git push" ve "git reset" bilerek disarida birakildi.
$izinliKomutlar = @(
    "dotnet",
    "git status", "git log", "git diff", "git show", "git branch",
    "git add", "git commit", "git switch", "git checkout", "git restore",
    "git stash", "git fetch", "git init", "git rev-parse", "git ls-files",
    "adb", "gh run", "gh pr", "gh workflow"
)

# Onek eslemeli yasak listesi. Izin on ayari ne olursa olsun bunlar sorulur.
$yasakliKomutlar = @(
    "git push --force",
    "git reset --hard",
    "git clean",
    "rm -rf",
    "diskpart",
    "Format-Volume",
    "shutdown"
)

function Ozellik-Ata($nesne, [string]$ad, $deger) {
    if ($null -eq $nesne.PSObject.Properties[$ad]) {
        $nesne | Add-Member -MemberType NoteProperty -Name $ad -Value $deger
    } else {
        $nesne.$ad = $deger
    }
}

function Liste-Birlestir($nesne, [string]$ad, [string[]]$yeni) {
    $mevcut = @()
    if ($null -ne $nesne.PSObject.Properties[$ad]) { $mevcut = @($nesne.$ad) }
    $birlesik = @($mevcut + $yeni | Where-Object { $_ } | Select-Object -Unique)
    Ozellik-Ata $nesne $ad $birlesik
    return ($birlesik.Count - $mevcut.Count)
}

function Json-Yaz($nesne, [string]$yol) {
    $damga = Get-Date -Format "yyyyMMdd-HHmmss"
    Copy-Item $yol "$yol.$damga.yedek"
    $metin = $nesne | ConvertTo-Json -Depth 100
    # Go tarafindaki ayristirici BOM kabul etmiyor: BOM'suz UTF-8 sart.
    [System.IO.File]::WriteAllText($yol, $metin, (New-Object System.Text.UTF8Encoding($false)))
    Write-Output "  yazildi : $yol"
    Write-Output "  yedek   : $yol.$damga.yedek"
}

Write-Output ""
Write-Output "== Genel ayar =="

$ayar = Get-Content $genelDosya -Raw | ConvertFrom-Json
if ($null -eq $ayar.PSObject.Properties["userSettings"]) {
    Ozellik-Ata $ayar "userSettings" ([pscustomobject]@{})
}
$us = $ayar.userSettings

$eklenenIzin  = Liste-Birlestir $us "allowedCommands" $izinliKomutlar
$eklenenYasak = Liste-Birlestir $us "deniedCommands"  $yasakliKomutlar

Write-Output "  izin listesine eklenen  : $eklenenIzin"
Write-Output "  yasak listesine eklenen : $eklenenYasak"

if ($Genel) {
    Ozellik-Ata $us "permissionPreset"    "AGENT_PERMISSION_PRESET_TURBO"
    Ozellik-Ata $us "autoExecutionPolicy" "CASCADE_COMMANDS_AUTO_EXECUTION_EAGER"
    Write-Output "  on ayar                 : TURBO (tum projeler)"
}

Json-Yaz $ayar $genelDosya

Write-Output ""
Write-Output "== Proje ayari: $Proje =="

$projeDosyalari = Get-ChildItem $projeDizin -Filter "*.json" -ErrorAction SilentlyContinue
$hedef = $null
foreach ($dosya in $projeDosyalari) {
    $icerik = Get-Content $dosya.FullName -Raw | ConvertFrom-Json
    if ($icerik.name -eq $Proje) { $hedef = @{ Yol = $dosya.FullName; Icerik = $icerik }; break }
}

if ($null -eq $hedef) {
    Write-Warning "'$Proje' adinda bir Antigravity projesi bulunamadi. Bulunanlar:"
    foreach ($dosya in $projeDosyalari) {
        $ad = (Get-Content $dosya.FullName -Raw | ConvertFrom-Json).name
        Write-Warning "  - $ad"
    }
    Write-Warning "Projeyi Antigravity'de bir kez acip betigi tekrar calistir."
    return
}

$projeAyari = $hedef.Icerik
if ($null -eq $projeAyari.PSObject.Properties["settings"] -or $null -eq $projeAyari.settings) {
    Ozellik-Ata $projeAyari "settings" ([pscustomobject]@{})
}
Ozellik-Ata $projeAyari.settings "permissionPreset"    "AGENT_PERMISSION_PRESET_TURBO"
Ozellik-Ata $projeAyari.settings "autoExecutionPolicy" "CASCADE_COMMANDS_AUTO_EXECUTION_EAGER"

Write-Output "  on ayar : TURBO (yalniz $Proje)"
Json-Yaz $projeAyari $hedef.Yol

Write-Output ""
Write-Output "Bitti. Antigravity'i ac ve bir komut calistirtip dogrula."
Write-Output "Geri almak icin .yedek uzantili dosyalari eski adlarina kopyala."
