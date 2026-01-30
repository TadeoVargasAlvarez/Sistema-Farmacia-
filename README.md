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

## Desplegar en Railway (URL pública + dominio)
1) Sube el proyecto a GitHub.
2) En Railway: **New Project → Deploy from GitHub repo** y selecciona el repo.
3) (Recomendado) Agrega un servicio **MySQL** en el mismo proyecto.
4) En tu servicio Web → **Variables**, configura:
	- `ASPNETCORE_ENVIRONMENT=Production`
	- `ConnectionStrings__Default=${{ MySQL.MYSQL_URL }}`
	- `FARMACIA_SEED_ADMIN_PASSWORD=UnaClaveFuerte` (antes del primer arranque)
5) Deploy. Railway te dará una URL pública.
6) Para usar tu dominio: **Settings → Domains → Add Domain** y sigue las instrucciones de DNS (CNAME/A).

## MySQL (Railway)
El proyecto puede conectarse a MySQL. En Railway normalmente obtienes una URL del tipo:

- `mysql://usuario:password@host:puerto/base`

En Railway puedes asignarla a tu app como variable de entorno usando el placeholder:

- `ConnectionStrings__Default = ${{ MySQL.MYSQL_URL }}`

En runtime la app recibe el valor real y lo convierte automáticamente.

Si prefieres usar un connection string clásico, usa:

`Server=HOST;Port=3306;Database=DB;User=USER;Password=PASS;SslMode=Required;`
