using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Areas.Inventarios.Controllers;

[Area("Inventarios")]
[Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
public class KardexController : Controller
{
    private readonly AppDbContext _db;

    public KardexController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(int? productoId, DateTime? desde, DateTime? hasta, string? tipo)
    {
        ViewBag.ActiveModule = "Inventarios";
        ViewBag.ActiveSubModule = "Kardex";

        await LoadProductosAsync(productoId);

        var query = _db.MovimientosInventario
            .AsNoTracking()
            .Include(x => x.Producto)
            .Include(x => x.Usuario)
            .AsQueryable();

        if (productoId.HasValue)
        {
            query = query.Where(x => x.ProductoId == productoId.Value);
        }

        if (desde.HasValue)
        {
            query = query.Where(x => x.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            var hastaInclusive = hasta.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.Fecha <= hastaInclusive);
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var t = tipo.Trim();
            query = query.Where(x => x.Tipo.Contains(t));
        }

        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Take(500)
            .ToListAsync();

        var vm = new KardexIndexVm
        {
            ProductoId = productoId,
            Desde = desde,
            Hasta = hasta,
            Tipo = tipo,
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

public class KardexIndexVm
{
    public int? ProductoId { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Tipo { get; set; }
    public List<MovimientoInventario> Items { get; set; } = new();
}
