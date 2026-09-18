param(
    [string]$UnityPath = ""
)

$ErrorActionPreference = "Stop"

$ProjectPath = $PSScriptRoot
$UnityVersion = "6000.6.1f1"
$DefaultUnityPath = Join-Path $env:ProgramFiles "Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    if (Test-Path $DefaultUnityPath) {
        $UnityPath = $DefaultUnityPath
    }
    else {
        $UnityCommand = Get-Command "Unity.exe" -ErrorAction SilentlyContinue
        if ($null -ne $UnityCommand) {
            $UnityPath = $UnityCommand.Source
        }
    }
}

if ([string]::IsNullOrWhiteSpace($UnityPath) -or -not (Test-Path $UnityPath)) {
    throw "Unity $UnityVersion was not found. Pass the path manually, for example: .\Build-Windows.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Unity.exe'"
}

$BuildLog = Join-Path $ProjectPath "Builds\Windows\windows-build.log"
$BuildDirectory = Split-Path $BuildLog -Parent
New-Item -ItemType Directory -Path $BuildDirectory -Force | Out-Null

Write-Host "Building Samurai Kurohane Prototype..."
Write-Host "Unity: $UnityPath"
Write-Host "Project: $ProjectPath"

& $UnityPath -batchmode -quit -projectPath $ProjectPath -executeMethod JMGameStudios.SamuraiKurohane.Editor.SamuraiKurohaneWindowsBuilder.BuildWindowsPrototype -logFile $BuildLog

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Unity build failed. Log: $BuildLog"
    exit $LASTEXITCODE
}

$ExePath = Join-Path $ProjectPath "Builds\Windows\SamuraiKurohanePrototype\SamuraiKurohanePrototype.exe"

if (-not (Test-Path $ExePath)) {
    throw "Unity exited successfully, but the expected EXE was not found: $ExePath"
}

Write-Host ""
Write-Host "Build complete:"
Write-Host $ExePath
