# Veracode Modular .NET Demo

PoC de una solucion .NET modular para explicar escaneo por unidades funcionales con Veracode.

## Estructura

```text
ModularVeracodeDemo/
  src/
    Shared/VeracodeDemo.Shared/
    Modules/
      Clientes/VeracodeDemo.Clientes.Api/
      Reservas/VeracodeDemo.Reservas.Api/
      Pagos/VeracodeDemo.Pagos.Api/
  scripts/
    package-module.ps1
    package-all.ps1
  .github/workflows/veracode-modular-scan.yml
  veracode.yml
  veracode.discover.example.yml
```

## Dependencias entre proyectos

- `VeracodeDemo.Shared`: modelos comunes reutilizados por todos los modulos.
- `VeracodeDemo.Clientes.Api`: API funcional de clientes, depende de `Shared`.
- `VeracodeDemo.Reservas.Api`: API funcional de reservas, depende de `Shared`.
- `VeracodeDemo.Pagos.Api`: API funcional de pagos, depende de `Shared`.

Cada modulo es una API ASP.NET Core independiente. Esto permite publicar y empaquetar cada unidad funcional por separado.

## Comandos base

Crear/restaurar/compilar:

```powershell
dotnet restore .\ModularVeracodeDemo.sln
dotnet build .\ModularVeracodeDemo.sln -c Release
```

Publicar un modulo:

```powershell
dotnet publish .\src\Modules\Clientes\VeracodeDemo.Clientes.Api\VeracodeDemo.Clientes.Api.csproj -c Release -o .\artifacts\publish\clientes
```

Generar los ZIP modulares para Veracode:

```powershell
.\scripts\package-module.ps1 -Module clientes
.\scripts\package-module.ps1 -Module reservas
.\scripts\package-module.ps1 -Module pagos
```

O todos a la vez:

```powershell
.\scripts\package-all.ps1
```

Los artefactos quedan en:

```text
artifacts/veracode/clientes.zip
artifacts/veracode/reservas.zip
artifacts/veracode/pagos.zip
```

## Veracode package discover / AutoPackager

La documentacion oficial de Veracode indica que `veracode package discover` detecta toolchains, identifica modulos soportados y genera o actualiza `veracode.yml`. Para .NET, soporta proyectos MSBuild con `.csproj` o `.publishproj`.

Pre-chequeo recomendado para esta estructura:

```bash
veracode package discover src --dry-run --format yaml
```

Generacion real de `veracode.yml` desde la CLI:

```bash
veracode package discover src
```

En esta PoC se deja `veracode.yml` en la raiz con las rutas limpias de los tres modulos. Si el cliente cambia la estructura del repo, conviene regenerarlo con `package discover` y revisar el diff.

Empaquetado con AutoPackager, despues de validar el contenido generado por discovery:

```bash
veracode package --source . --trust
```

Validado localmente con Veracode CLI v2.52.0: el comando creo tres artefactos modulares:

```text
veracode-auto-pack-VeracodeDemo.Clientes.Api-dotnet.zip
veracode-auto-pack-VeracodeDemo.Reservas.Api-dotnet.zip
veracode-auto-pack-VeracodeDemo.Pagos.Api-dotnet.zip
```

Importante: el archivo `veracode.discover.example.yml` incluido aqui es solamente una guia para conversar con el cliente. La salida autoritativa debe producirla la CLI de Veracode en el entorno real.

## Escaneo modular en GitHub Actions

El workflow `.github/workflows/veracode-modular-scan.yml` usa una matriz con tres modulos:

- `clientes`
- `reservas`
- `pagos`

Cada job publica y empaqueta su modulo de forma independiente. El bloque de Pipeline Scan queda como placeholder para reemplazarlo por la accion oficial o el comando JAR aprobado por el cliente, usando estos secretos:

```text
VERACODE_API_ID
VERACODE_API_KEY
```

La parte importante de la PoC es que la matriz mantiene un escaneo por modulo, permitiendo ejecucion paralela y comparacion contra un escaneo monolitico.

## Fuentes oficiales consultadas

- Veracode CLI `package discover`: https://docs.veracode.com/r/veracode_package_discover
- About package discover: https://docs.veracode.com/r/About_package_discover
- About autopackaging: https://docs.veracode.com/r/About_auto_packaging
