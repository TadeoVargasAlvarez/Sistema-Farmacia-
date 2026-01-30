using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Areas.Inventarios.Controllers;

[Area("Inventarios")]
[Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
public class MarcasController : Controller
{
    private readonly AppDbContext _db;

    public MarcasController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Marcas";

        var items = await _db.Marcas.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Marcas";
        return View(new Marca());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Marca model)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Marcas";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Marcas.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Marcas";

        var item = await _db.Marcas.FindAsync(id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Marca model)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Marcas";

        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Marcas.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Marcas";

        var item = await _db.Marcas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.Marcas.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Index));

        _db.Marcas.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
