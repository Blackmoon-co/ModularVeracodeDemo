param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("clientes", "reservas", "pagos")]
    [string] $Module,

    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$projects = @{
    clientes = "src/Modules/Clientes/VeracodeDemo.Clientes.Api/VeracodeDemo.Clientes.Api.csproj"
    reservas = "src/Modules/Reservas/VeracodeDemo.Reservas.Api/VeracodeDemo.Reservas.Api.csproj"
    pagos = "src/Modules/Pagos/VeracodeDemo.Pagos.Api/VeracodeDemo.Pagos.Api.csproj"
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot $projects[$Module]
$publishPath = Join-Path $repoRoot "artifacts/publish/$Module"
$zipPath = Join-Path $repoRoot "artifacts/veracode/$Module.zip"

Remove-Item -Recurse -Force $publishPath -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $publishPath | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path $zipPath) | Out-Null

dotnet publish $projectPath `
    --configuration $Configuration `
    --output $publishPath `
    /p:UseAppHost=false `
    /p:DebugType=portable

Remove-Item -Force $zipPath -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $publishPath "*") -DestinationPath $zipPath -Force

Write-Host "Paquete modular generado: $zipPath"
