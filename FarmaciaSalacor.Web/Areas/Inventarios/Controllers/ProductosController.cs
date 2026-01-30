using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Areas.Inventarios.Controllers;

[Area("Inventarios")]
public class ProductosController : Controller
{
    private readonly AppDbContext _db;

    public ProductosController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Productos";

        var items = await _db.Productos
            .AsNoTracking()
            .Include(x => x.Categoria)
            .Include(x => x.Marca)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return View(items);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Productos";

        await LoadCatalogosAsync();
        return View(new Producto());
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto model)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Productos";

        if (!ModelState.IsValid)
        {
            await LoadCatalogosAsync();
            return View(model);
        }

        _db.Productos.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Productos";

        var item = await _db.Productos.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await LoadCatalogosAsync();
        return View(item);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto model)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Productos";

        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadCatalogosAsync();
            return View(model);
        }

        _db.Productos.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Productos";

        var item = await _db.Productos
            .AsNoTracking()
            .Include(x => x.Categoria)
            .Include(x => x.Marca)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Productos.FindAsync(id);
        if (item is null)
        {
            return RedirectToAction(nameof(Index));
        }

        _db.Productos.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCatalogosAsync()
    {
        var categorias = await _db.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        var marcas = await _db.Marcas.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();

        ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre");
        ViewBag.Marcas = new SelectList(marcas, "Id", "Nombre");
    }
}
