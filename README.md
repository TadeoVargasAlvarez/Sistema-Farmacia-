# Farmacia Salacor (ERP)

ERP web para farmacia en **ASP.NET Core MVC (.NET 8)** con **EF Core + SQLite**, autenticación por cookies y roles.

## Requisitos
- .NET SDK 8
- (Opcional) Git, para versionar y subir a GitHub

## Ejecutar en local
Desde la carpeta raíz:

```bash
cd FarmaciaSalacor.Web
dotnet run
```

La app aplica migraciones automáticamente y crea un usuario inicial si la base está vacía.

## Usuario inicial (primer arranque)
- Usuario: `admin`
- Contraseña por defecto: `123456`

Si quieres definir tu propia contraseña inicial antes del primer arranque, establece la variable de entorno:

- `FARMACIA_SEED_ADMIN_PASSWORD`

Ejemplo (PowerShell):

```powershell
$env:FARMACIA_SEED_ADMIN_PASSWORD = "UnaClaveFuerte"; dotnet run
```

## Notas de despliegue
- Para usar con dominio, publica con `dotnet publish` y monta en IIS o Linux+Nginx.
- En producción usa HTTPS.
