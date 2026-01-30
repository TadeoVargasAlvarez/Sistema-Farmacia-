using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Controllers;

public class AlmacenesController : Controller
{
    private readonly AppDbContext _db;

    public AlmacenesController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveModule = "Almacenes";
        ViewBag.ActiveSubModule = "Catalogo";

        var items = await _db.Almacenes.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return View(items);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.ActiveModule = "Almacenes";
        ViewBag.ActiveSubModule = "Catalogo";
        return View(new Almacen());
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Almacen model)
    {
        ViewBag.ActiveModule = "Almacenes";
        ViewBag.ActiveSubModule = "Catalogo";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Almacenes.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.ActiveModule = "Almacenes";
        ViewBag.ActiveSubModule = "Catalogo";

        var item = await _db.Almacenes.FindAsync(id);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Almacen model)
    {
        ViewBag.ActiveModule = "Almacenes";
        ViewBag.ActiveSubModule = "Catalogo";

        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Almacenes.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewBag.ActiveModule = "Almacenes";
        ViewBag.ActiveSubModule = "Catalogo";

        var item = await _db.Almacenes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        ViewBag.ActiveModule = "Almacenes";
        ViewBag.ActiveSubModule = "Catalogo";

        var item = await _db.Almacenes.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Index));

        _db.Almacenes.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

