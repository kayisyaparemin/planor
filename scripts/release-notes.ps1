<#
.SYNOPSIS
    Bir sürümün notunu CHANGELOG.md'den okur ve doğrular (K4, S82).

.DESCRIPTION
    Notu iş akışında elle üretmek yerine repodaki CHANGELOG.md'nin etiketin sürümüne ait bölümünü okur.
    Okuma kuralları tools/Mizan.ReleaseNotes içindedir ve Mizan.Regression.Tests'te negatif durumlarla
    sınanır: bölüm yoksa, boşsa, başlığı ya da tarihi okunamıyorsa ya da sürüm iki kez geçiyorsa
    betik hata ile biter. -Output verilirse not o dosyaya yazılır; hata olursa eski çıktı da silinir.

.PARAMETER Tag
    Yayın etiketi (vX.Y.Z).

.PARAMETER Changelog
    Okunacak dosya. Varsayılan: CHANGELOG.md (depo kökü)

.PARAMETER Output
    Notun yazılacağı dosya (gh release create --notes-file için).
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)][string]$Tag,
    [string]$Changelog,
    [string]$Output
)

$ErrorActionPreference = "Stop"
# Windows PowerShell 5.1'de $PSScriptRoot param varsayılanlarında boştur; yol burada çözülür.
if (-not $Changelog) { $Changelog = Join-Path $PSScriptRoot "..\CHANGELOG.md" }

$toolArgs = @("--changelog", $Changelog, "--version", $Tag)
if ($Output) { $toolArgs += @("--output", $Output) }

$project = Join-Path $PSScriptRoot "..\tools\Mizan.ReleaseNotes\Mizan.ReleaseNotes.csproj"
& dotnet run --project $project -c Release --nologo -v q -- @toolArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "Sürüm notu okunamadı; yayın yapılmayacak." -ForegroundColor Red
    exit 1
}
exit 0
