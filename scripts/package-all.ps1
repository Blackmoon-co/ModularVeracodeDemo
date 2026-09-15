$ErrorActionPreference = "Stop"

foreach ($module in @("clientes", "reservas", "pagos")) {
    & "$PSScriptRoot/package-module.ps1" -Module $module
}

Write-Host "Paquetes listos en artifacts/veracode"
