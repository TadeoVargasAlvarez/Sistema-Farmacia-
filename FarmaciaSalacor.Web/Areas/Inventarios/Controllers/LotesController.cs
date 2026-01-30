using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Areas.Inventarios.Controllers;

[Area("Inventarios")]
[Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
public class LotesController : Controller
{
    private readonly AppDbContext _db;

    public LotesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Lotes";

        var items = await _db.Lotes
            .AsNoTracking()
            .Include(x => x.Producto)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Lotes";

        await LoadProductosAsync();
        return View(new Lote { Activo = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Lote model)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Lotes";

        if (!ModelState.IsValid)
        {
            await LoadProductosAsync(model.ProductoId);
            return View(model);
        }

        _db.Lotes.Add(model);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un lote con ese número para el producto seleccionado.");
            await LoadProductosAsync(model.ProductoId);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Lotes";

        var item = await _db.Lotes.FindAsync(id);
        if (item is null) return NotFound();

        await LoadProductosAsync(item.ProductoId);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Lote model)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Lotes";

        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await LoadProductosAsync(model.ProductoId);
            return View(model);
        }

        _db.Lotes.Update(model);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un lote con ese número para el producto seleccionado.");
            await LoadProductosAsync(model.ProductoId);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Lotes";

        var item = await _db.Lotes
            .AsNoTracking()
            .Include(x => x.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Lotes.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Index));

        _db.Lotes.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadProductosAsync(int? selectedId = null)
    {
        var productos = await _db.Productos.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Productos = new SelectList(productos, "Id", "Nombre", selectedId);
    }
}
