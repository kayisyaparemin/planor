<#
.SYNOPSIS
    Bir Planör APK'sının kimliğini, imzasını ve çevrimdışılığını doğrular (K2, S80).

.DESCRIPTION
    Android build-tools'tan en yeni aapt2 ve apksigner'ı bulur, üç kanıtı toplar
    (badging, sertifikalar, birleşmiş manifest) ve kuralları tools/Mizan.ApkVerifier ile çalıştırır.
    Kurallar orada tek yerdedir ve Mizan.Regression.Tests'te negatif durumlarla sınanır.
    Araç bulunamazsa ya da bir kanıt okunamazsa sonuç "doğrulanamadı"dır, asla "geçti" değildir.

.PARAMETER Apk
    Doğrulanacak imzalı APK.

.PARAMETER Csproj
    Beklenen kimliğin okunacağı proje. Varsayılan: src/Mizan.App/Mizan.App.csproj

.PARAMETER Tag
    Yayın etiketi (vX.Y.Z). Verilirse csproj'daki ApplicationDisplayVersion ile eşleşmelidir.

.PARAMETER RequireReleaseSignature
    Debug sertifikasını reddeder. Yalnız yayın iş akışında verilir.
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)][string]$Apk,
    [string]$Csproj,
    [string]$Tag,
    [switch]$RequireReleaseSignature
)

$ErrorActionPreference = "Stop"
# Windows PowerShell 5.1'de $PSScriptRoot param varsayılanlarında boştur; yol burada çözülür.
if (-not $Csproj) { $Csproj = Join-Path $PSScriptRoot "..\src\Mizan.App\Mizan.App.csproj" }
$onWindows = [System.IO.Path]::DirectorySeparatorChar -eq '\'

function Find-BuildTool([string]$name) {
    $roots = @($env:ANDROID_HOME, $env:ANDROID_SDK_ROOT)
    if ($env:LOCALAPPDATA) { $roots += (Join-Path $env:LOCALAPPDATA "Android\Sdk") }
    $searched = @()
    foreach ($root in ($roots | Where-Object { $_ })) {
        $buildTools = Join-Path $root "build-tools"
        $searched += $buildTools
        if (-not (Test-Path $buildTools)) { continue }
        $versions = Get-ChildItem $buildTools -Directory |
            Where-Object { $_.Name -match '^\d+(\.\d+)*$' } |
            Sort-Object { [version]$_.Name } -Descending
        foreach ($version in $versions) {
            foreach ($candidate in @("$name.exe", "$name.bat", $name)) {
                $path = Join-Path $version.FullName $candidate
                if (Test-Path $path -PathType Leaf) { return $path }
            }
        }
    }
    throw "'$name' bulunamadı; APK doğrulanamadı. Aranan yerler: $($searched -join ', ')"
}

function Invoke-Tool([string]$tool, [string[]]$arguments, [string]$outFile) {
    $quoted = $arguments | ForEach-Object { if ($_ -match '\s') { "`"$_`"" } else { $_ } }
    $process = Start-Process -FilePath $tool -ArgumentList $quoted -NoNewWindow -Wait -PassThru `
        -RedirectStandardOutput $outFile -RedirectStandardError "$outFile.err"
    return $process.ExitCode
}

if (-not (Test-Path $Apk -PathType Leaf)) { throw "APK bulunamadı: $Apk" }
$apkPath = (Resolve-Path $Apk).Path
$aapt2 = Find-BuildTool "aapt2"
$apksigner = Find-BuildTool "apksigner"
Write-Host "APK      : $apkPath" -ForegroundColor Cyan
Write-Host "aapt2    : $aapt2"
Write-Host "apksigner: $apksigner"

$work = Join-Path ([System.IO.Path]::GetTempPath()) ("mizan-apk-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $work | Out-Null
try {
    $badging = Join-Path $work "badging.txt"
    $certs = Join-Path $work "certs.txt"
    $manifest = Join-Path $work "manifest.txt"

    if ((Invoke-Tool $aapt2 @("dump", "badging", $apkPath) $badging) -ne 0) { throw "aapt2 dump badging başarısız." }
    if ((Invoke-Tool $aapt2 @("dump", "xmltree", "--file", "AndroidManifest.xml", $apkPath) $manifest) -ne 0) { throw "aapt2 dump xmltree başarısız." }
    $signatureVerified = (Invoke-Tool $apksigner @("verify", "--print-certs", $apkPath) $certs) -eq 0

    $verifierArgs = @(
        "--badging", $badging, "--certs", $certs, "--manifest", $manifest,
        "--signature-verified", $signatureVerified.ToString().ToLowerInvariant(),
        "--csproj", (Resolve-Path $Csproj).Path)
    if ($Tag) { $verifierArgs += @("--tag", $Tag) }
    if ($RequireReleaseSignature) { $verifierArgs += "--require-release-signature" }

    $project = Join-Path $PSScriptRoot "..\tools\Mizan.ApkVerifier\Mizan.ApkVerifier.csproj"
    & dotnet run --project $project -c Release --nologo -v q -- @verifierArgs
    $exitCode = $LASTEXITCODE
}
finally {
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}

if ($exitCode -ne 0) {
    Write-Host "APK doğrulaması başarısız." -ForegroundColor Red
    exit 1
}
Write-Host "APK doğrulaması geçti." -ForegroundColor Green
exit 0
