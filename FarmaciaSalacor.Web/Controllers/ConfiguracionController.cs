using System.Security.Claims;
using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using FarmaciaSalacor.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Controllers;

public class ConfiguracionController : Controller
{
    private readonly AppDbContext _db;

    public ConfiguracionController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = Roles.Admin)]
    public IActionResult Index()
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";
        return RedirectToAction(nameof(Usuarios));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Usuarios()
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";

        var items = await _db.Usuarios.AsNoTracking().OrderBy(x => x.Username).ToListAsync();
        return View(items);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public IActionResult UsuarioCreate()
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";
        LoadRoles();
        return View(new UsuarioFormViewModel { Activo = true, Rol = Roles.Cajero });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UsuarioCreate(UsuarioFormViewModel model)
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";
        LoadRoles(model.Rol);

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "La contraseña es obligatoria.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.Username.Trim();
        var exists = await _db.Usuarios.AnyAsync(x => x.Username == username);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Username), "El usuario ya existe.");
            return View(model);
        }

        var user = new Usuario
        {
            Username = username,
            NombreCompleto = string.IsNullOrWhiteSpace(model.NombreCompleto) ? null : model.NombreCompleto.Trim(),
            Rol = model.Rol,
            Activo = model.Activo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Usuarios));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> UsuarioEdit(int id)
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";

        var user = await _db.Usuarios.FindAsync(id);
        if (user is null) return NotFound();

        LoadRoles(user.Rol);
        return View(new UsuarioFormViewModel
        {
            Id = user.Id,
            Username = user.Username,
            NombreCompleto = user.NombreCompleto,
            Rol = user.Rol,
            Activo = user.Activo
        });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UsuarioEdit(int id, UsuarioFormViewModel model)
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";
        LoadRoles(model.Rol);

        if (model.Id != id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Usuarios.FindAsync(id);
        if (user is null) return NotFound();

        var username = model.Username.Trim();
        var exists = await _db.Usuarios.AnyAsync(x => x.Username == username && x.Id != id);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Username), "El usuario ya existe.");
            return View(model);
        }

        user.Username = username;
        user.NombreCompleto = string.IsNullOrWhiteSpace(model.NombreCompleto) ? null : model.NombreCompleto.Trim();
        user.Rol = model.Rol;
        user.Activo = model.Activo;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Usuarios));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> UsuarioResetPassword(int id)
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";

        var user = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return NotFound();

        return View(new UsuarioResetPasswordViewModel { Id = user.Id, Username = user.Username });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UsuarioResetPassword(UsuarioResetPasswordViewModel model)
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";

        if (!ModelState.IsValid) return View(model);

        var user = await _db.Usuarios.FindAsync(model.Id);
        if (user is null) return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Usuarios));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> UsuarioDelete(int id)
    {
        ViewBag.ActiveModule = "Configuración";
        ViewBag.ActiveSubModule = "Usuarios";

        var user = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (user is null) return NotFound();
        return View(user);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UsuarioDeleteConfirmed(int id)
    {
        var currentUserId = GetUserId();
        if (currentUserId == id)
        {
            TempData["Error"] = "No puedes eliminar tu propio usuario.";
            return RedirectToAction(nameof(Usuarios));
        }

        var user = await _db.Usuarios.FindAsync(id);
        if (user is null) return RedirectToAction(nameof(Usuarios));

        _db.Usuarios.Remove(user);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Usuarios));
    }

    private void LoadRoles(string? selected = null)
    {
        var roles = new[] { Roles.Admin, Roles.Cajero, Roles.Almacen };
        ViewBag.Roles = new SelectList(roles, selected ?? Roles.Cajero);
    }

    private int? GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(str, out var id)) return id;
        return null;
    }
}

