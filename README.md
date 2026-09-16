# Veracode Modular .NET Demo

PoC de una solucion .NET modular para explicar escaneo por unidades funcionales con Veracode: cada modulo se empaqueta y escanea por separado en CI, en vez de un escaneo monolitico de todo el repo.

## Pipelines (GitHub Actions)

Hay 4 workflows en `.github/workflows/`. Uno es el orquestador que dispara todo en cada push a `main`; los otros tres son reusables (`workflow_call`) e invocados desde el orquestador o entre si.

### 1. `veracode-modular-scan.yml` — orquestador

Se dispara en `push` a `main`. Flujo:

1. **`detect-changes`**: hace diff entre el commit anterior y el actual y decide, por modulo, si hubo cambios (`clientes`, `reservas`, `pagos`). Si cambia algo compartido (`src/Shared/*`, `*.sln`, `scripts/*`, `Directory.Build.*`, `global.json`, `NuGet.config`, `Dockerfile`, etc.) marca `all_modules=true` y fuerza los tres.
2. **`package-clientes` / `package-reservas` / `package-pagos`**: si el modulo cambio, llaman a `veracode-autopackage.yml` para ese modulo (paralelo entre modulos).
3. **`scan-clientes` / `scan-reservas` / `scan-pagos`**: si el empaquetado del modulo tuvo exito, llaman a `veracode-pipeline-scan.yml` con `policy_name: "Politica Restrictiva"` y `fail_build: false`.

Es decir: **solo se empaqueta y escanea el modulo que cambio**, no los tres. Cada modulo tiene su propio par de jobs (`package-<modulo>` + `scan-<modulo>`) y ese par solo se ejecuta si `detect-changes` marco ese modulo en `true`. Los `if:` de cada job son independientes entre si, por eso en un push que solo toca `Pagos` los jobs de `clientes` y `reservas` ni siquiera arrancan (quedan "Skipped" en el run de Actions), y el tiempo de CI y el costo de escaneo se reducen a lo que realmente cambio.

Ejemplos de que dispara un push a `main`:

| Archivos modificados | `clientes` | `reservas` | `pagos` | Jobs que corren |
|---|---|---|---|---|
| `src/Modules/Pagos/VeracodeDemo.Pagos.Api/Program.cs` | false | false | true | `package-pagos` → `scan-pagos` |
| `src/Modules/Clientes/.../ClienteController.cs` | true | false | false | `package-clientes` → `scan-clientes` |
| `src/Modules/Reservas/...` + `src/Modules/Pagos/...` en el mismo push | false | true | true | `package-reservas` → `scan-reservas`, `package-pagos` → `scan-pagos` |
| `src/Shared/VeracodeDemo.Shared/...` (o `.sln`, `scripts/*`, `global.json`, etc.) | true | true | true | los 6 jobs (`package-*` → `scan-*` de los tres modulos) |
| Solo `README.md` u otro archivo fuera de `src/` y `scripts/` | false | false | false | ninguno — `detect-changes` corre pero ningun `package-*`/`scan-*` se dispara |

La logica vive en el paso `Detectar modulos modificados` de `detect-changes` (`veracode-modular-scan.yml:37-154`): compara `git diff` entre el commit anterior y el actual, y por cada archivo cambiado hace un `case` sobre la ruta para prender el flag del modulo correspondiente (o `all_modules` si es algo compartido, lo que a su vez prende los tres).

### 2. `veracode-autopackage.yml` — empaquetado (reusable)

Recibe `module_name`, `source_path`, `output_name`, `dotnet_version`. Instala la Veracode CLI, valida que exista `source_path`, corre `veracode package --source <path> --type directory --trust`, copia el ZIP resultante a `artifacts/veracode/final/<output_name>.zip` y lo sube como artifact `veracode-<output_name>`.

### 3. `veracode-pipeline-scan.yml` — Pipeline Scan (reusable)

Descarga el artifact generado en el paso anterior, ubica el `.zip`/`.war`/`.jar`/`.ear` y ejecuta `veracode/Veracode-pipeline-scan-action@v1.0.18` contra la politica indicada. Sube los resultados (JSON) como artifact `veracode-pipeline-scan-results-<artifact_name>`.

Requiere estos secrets del repo:

```text
VERACODE_API_ID_REPO
VERACODE_API_KEY_SECRET_REPO
```

### 4. `veracode-sca-scan.yml` — SCA (reusable, no conectado)

Corre `veracode/veracode-sca@v2.1.10` (analisis de dependencias open source) sobre una `path` dada, usando el secret `VERACODE_AGENT_TOKEN`. Existe en el repo pero **ningun workflow lo invoca todavia** — queda listo para engancharlo al orquestador cuando se quiera sumar SCA por modulo.

### Diagrama del flujo

```text
push a main
  -> detect-changes (que modulos cambiaron)
       -> package-<modulo> (veracode-autopackage.yml)
            -> scan-<modulo> (veracode-pipeline-scan.yml)

veracode-sca-scan.yml  (reusable suelto, sin caller todavia)
```

## Estructura del repo

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
  .github/workflows/
    veracode-modular-scan.yml      # orquestador (push a main)
    veracode-autopackage.yml       # reusable: empaquetado por modulo
    veracode-pipeline-scan.yml     # reusable: Pipeline Scan por modulo
    veracode-sca-scan.yml          # reusable: SCA (sin conectar)
  veracode.yml
  veracode.discover.example.yml
```

`artifacts/` (publish + zips) y `veracode-auto-pack-*.zip` son salida local/CI y estan en `.gitignore`, no se versionan.

## Dependencias entre proyectos

- `VeracodeDemo.Shared`: modelos comunes reutilizados por todos los modulos.
- `VeracodeDemo.Clientes.Api`: API funcional de clientes, depende de `Shared`.
- `VeracodeDemo.Reservas.Api`: API funcional de reservas, depende de `Shared`.
- `VeracodeDemo.Pagos.Api`: API funcional de pagos, depende de `Shared`.

Cada modulo es una API ASP.NET Core (.NET 8) independiente, lo que permite publicar y empaquetar cada unidad funcional por separado — la base de por que el escaneo modular tiene sentido.

## Comandos locales

Restaurar/compilar:

```powershell
dotnet restore .\ModularVeracodeDemo.sln
dotnet build .\ModularVeracodeDemo.sln -c Release
```

Publicar un modulo:

```powershell
dotnet publish .\src\Modules\Clientes\VeracodeDemo.Clientes.Api\VeracodeDemo.Clientes.Api.csproj -c Release -o .\artifacts\publish\clientes
```

Generar los ZIP modulares para Veracode (publish + compress, equivalente a lo que hace CI con la Veracode CLI):

```powershell
.\scripts\package-module.ps1 -Module clientes
.\scripts\package-module.ps1 -Module reservas
.\scripts\package-module.ps1 -Module pagos
```

O todos a la vez:

```powershell
.\scripts\package-all.ps1
```

Los artefactos quedan en `artifacts/veracode/<modulo>.zip` (no versionado).

## Veracode package discover / AutoPackager (CLI local)

La documentacion oficial de Veracode indica que `veracode package discover` detecta toolchains, identifica modulos soportados y genera o actualiza `veracode.yml`. Para .NET soporta proyectos MSBuild con `.csproj` o `.publishproj`. En este repo `veracode.yml` ya esta generado en la raiz con las rutas de los tres modulos (ver arriba); si cambia la estructura conviene regenerarlo:

```bash
veracode package discover src --dry-run --format yaml   # pre-chequeo
veracode package discover src                            # genera veracode.yml
veracode package --source . --trust                      # AutoPackager, genera un zip por modulo
```

`veracode.discover.example.yml` es solo una guia de referencia para conversar con el cliente; la salida autoritativa la produce la CLI en el entorno real. Esto es lo mismo que hace `veracode-autopackage.yml` en CI, pero corrido en local.

## Fuentes oficiales consultadas

- Veracode CLI `package discover`: https://docs.veracode.com/r/veracode_package_discover
- About package discover: https://docs.veracode.com/r/About_package_discover
- About autopackaging: https://docs.veracode.com/r/About_auto_packaging
