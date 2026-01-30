using FarmaciaSalacor.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Railway expone el puerto en la variable PORT. Kestrel debe escuchar en 0.0.0.0.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

var defaultConnection = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException("Falta ConnectionStrings:Default en appsettings.json");
}

static bool LooksLikeMySqlUrl(string value)
    => value.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase)
       || value.StartsWith("mariadb://", StringComparison.OrdinalIgnoreCase);

static string MySqlUrlToConnectionString(string url)
{
    // Formato típico Railway: mysql://user:pass@host:port/db
    // Nota: password y user pueden venir url-encoded.
    var uri = new Uri(url);

    var userInfo = uri.UserInfo.Split(':', 2);
    var user = userInfo.Length > 0 ? WebUtility.UrlDecode(userInfo[0]) : string.Empty;
    var pass = userInfo.Length > 1 ? WebUtility.UrlDecode(userInfo[1]) : string.Empty;

    var database = uri.AbsolutePath.Trim('/');
    if (string.IsNullOrWhiteSpace(database))
    {
        throw new InvalidOperationException("MYSQL_URL inválida: falta el nombre de la base de datos en la URL.");
    }

    var port = uri.IsDefaultPort ? 3306 : uri.Port;

    // Recomendación en servicios cloud: SSL requerido.
    return $"Server={uri.Host};Port={port};Database={database};User={user};Password={pass};SslMode=Required;";
}

var isMySql = LooksLikeMySqlUrl(defaultConnection)
    || defaultConnection.Contains("Server=", StringComparison.OrdinalIgnoreCase)
    || defaultConnection.Contains("Host=", StringComparison.OrdinalIgnoreCase);

var connectionToUse = defaultConnection;
if (LooksLikeMySqlUrl(connectionToUse))
{
    connectionToUse = MySqlUrlToConnectionString(connectionToUse);
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (isMySql)
    {
        options.UseMySql(connectionToUse, ServerVersion.AutoDetect(connectionToUse));
    }
    else
    {
        options.UseSqlite(connectionToUse);
    }
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Denied";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Healthcheck para Railway: debe responder 200 rápido y sin redirecciones.
// Se ubica antes de UseHttpsRedirection/Auth para evitar 30x/401.
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/health", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync("OK");
        return;
    }

    await next();
});

// Railway y otros PaaS suelen pasar la IP y el esquema real por headers.
// Esto evita bucles con UseHttpsRedirection cuando hay terminación TLS en el proxy.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Aceptar forwarded headers en entornos detrás de proxy (Railway).
    KnownNetworks = { },
    KnownProxies = { }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();

    // Para SQLite usamos migraciones.
    // Para MySQL en Railway, ConnectionStrings:Default suele venir como mysql://...;
    // las migraciones actuales fueron generadas para SQLite (tipos INTEGER/TEXT),
    // por eso en MySQL creamos el esquema a partir del modelo.
    if (db.Database.IsMySql())
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    await DbSeeder.SeedAsync(db);
}

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
