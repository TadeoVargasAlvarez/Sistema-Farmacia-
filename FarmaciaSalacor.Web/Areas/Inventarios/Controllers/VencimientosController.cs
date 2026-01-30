using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Areas.Inventarios.Controllers;

[Area("Inventarios")]
[Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
public class VencimientosController : Controller
{
    private readonly AppDbContext _db;

    public VencimientosController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int? productoId, int? dias)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Vencimientos";

        var diasEval = dias ?? 30;
        if (diasEval < 0) diasEval = 0;
        if (diasEval > 365) diasEval = 365;

        await LoadProductosAsync(productoId);

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var limite = hoy.AddDays(diasEval);

        var query = _db.Lotes
            .AsNoTracking()
            .Include(x => x.Producto)
            .Where(x => x.Vencimiento != null)
            .AsQueryable();

        if (productoId.HasValue)
        {
            query = query.Where(x => x.ProductoId == productoId.Value);
        }

        query = query.Where(x => x.Vencimiento <= limite);

        var items = await query
            .OrderBy(x => x.Vencimiento)
            .ThenBy(x => x.Producto!.Nombre)
            .Take(500)
            .ToListAsync();

        var vm = new VencimientosIndexVm
        {
            ProductoId = productoId,
            Dias = diasEval,
            Hoy = hoy,
            Items = items
        };

        return View(vm);
    }

    private async Task LoadProductosAsync(int? selectedId = null)
    {
        var productos = await _db.Productos.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Productos = new SelectList(productos, "Id", "Nombre", selectedId);
    }
}

public class VencimientosIndexVm
{
    public int? ProductoId { get; set; }
    public int Dias { get; set; }
    public DateOnly Hoy { get; set; }
    public List<Lote> Items { get; set; } = new();
}
