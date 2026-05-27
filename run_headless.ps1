param(
    [switch]$ExitWhenDone
)

$modDir  = $PSScriptRoot
$gameExe = "C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\SlayTheSpire2.exe"

Set-Location $modDir
dotnet build
if ($LASTEXITCODE -eq 0) {
    $env:STS2_EXIT_WHEN_DONE = if ($ExitWhenDone) { "1" } else { "0" }
    & $gameExe --headless
}
