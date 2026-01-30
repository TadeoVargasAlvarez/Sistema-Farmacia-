using FarmaciaSalacor.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Usuarios.AnyAsync())
        {
            return;
        }

        var seedPassword = Environment.GetEnvironmentVariable("FARMACIA_SEED_ADMIN_PASSWORD");
        if (string.IsNullOrWhiteSpace(seedPassword))
        {
            seedPassword = "123456";
        }

        var admin = new Usuario
        {
            Username = "admin",
            NombreCompleto = "Administrador",
            Rol = Roles.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedPassword)
        };

        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();
    }
}
