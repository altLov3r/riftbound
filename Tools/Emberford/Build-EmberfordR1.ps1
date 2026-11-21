param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.6.2f1\Editor\Unity.exe",
    [string]$ProjectPath = (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent),
    [string]$ServerOut = "Builds/EmberfordServer01",
    [string]$ClientOut = "Builds/EmberfordClient01",
    [switch]$ClientOnly,
    [switch]$ServerOnly
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path (Join-Path $ProjectPath ".code-check/emberford-r1") | Out-Null

if (-not $ServerOnly) {
    Write-Host "Building Emberford Windows x64 Client -> $ClientOut/Riftbound.exe"
    $pCli = Start-Process -FilePath $UnityPath -ArgumentList "-batchmode", "-quit", "-projectPath", $ProjectPath, "-executeMethod", "Riftbound.Editor.EmberfordRealmBuild.BuildClient", "-emberfordClientOut", $ClientOut, "-logFile", (Join-Path $ProjectPath ".code-check/emberford-r1/build-client.log") -Wait -PassThru -NoNewWindow
    Write-Host "CLIENT_BUILD_EXIT=$($pCli.ExitCode)"
    if ($pCli.ExitCode -ne 0) { exit $pCli.ExitCode }
}

if (-not $ClientOnly) {
    Write-Host "Building Emberford Windows x64 Dedicated Server (StandaloneBuildSubtarget.Server) -> $ServerOut/EmberfordServer.exe"
    $pSrv = Start-Process -FilePath $UnityPath -ArgumentList "-batchmode", "-quit", "-projectPath", $ProjectPath, "-executeMethod", "Riftbound.Editor.EmberfordRealmBuild.BuildDedicatedServer", "-emberfordServerOut", $ServerOut, "-logFile", (Join-Path $ProjectPath ".code-check/emberford-r1/build-server.log") -Wait -PassThru -NoNewWindow
    Write-Host "SERVER_BUILD_EXIT=$($pSrv.ExitCode)"
    if ($pSrv.ExitCode -ne 0) { exit $pSrv.ExitCode }
}
