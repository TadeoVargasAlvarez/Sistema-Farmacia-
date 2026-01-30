using System.Security.Claims;
using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using FarmaciaSalacor.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Controllers;

public class TransferenciasController : Controller
{
    private readonly AppDbContext _db;

    public TransferenciasController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    public IActionResult Index()
    {
        ViewBag.ActiveModule = "Transferencias";
        ViewBag.ActiveSubModule = "Historial";
        return RedirectToAction(nameof(Historial));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Nueva()
    {
        ViewBag.ActiveModule = "Transferencias";
        ViewBag.ActiveSubModule = "Nueva";

        await LoadAlmacenesAsync();
        await LoadProductosAsync();

        var vm = new TransferenciaNuevaViewModel
        {
            Items = new List<TransferenciaNuevaItemViewModel> { new() { Cantidad = 1 } }
        };
        return View(vm);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(TransferenciaNuevaViewModel model)
    {
        ViewBag.ActiveModule = "Transferencias";
        ViewBag.ActiveSubModule = "Nueva";

        model.Items = model.Items.Where(x => x.ProductoId > 0 && x.Cantidad > 0).ToList();

        if (model.DesdeAlmacenId == model.HaciaAlmacenId)
        {
            ModelState.AddModelError(string.Empty, "El almacén de origen y destino no pueden ser el mismo.");
        }

        if (model.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Agrega al menos un producto.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAlmacenesAsync(model.DesdeAlmacenId, model.HaciaAlmacenId);
            await LoadProductosAsync();
            return View(model);
        }

        var userId = GetUserId();
        if (userId is null) return Forbid();

        var productoIds = model.Items.Select(x => x.ProductoId).Distinct().ToList();
        var productos = await _db.Productos.Where(x => productoIds.Contains(x.Id)).ToListAsync();
        if (productos.Count != productoIds.Count)
        {
            ModelState.AddModelError(string.Empty, "Producto inválido.");
            await LoadAlmacenesAsync(model.DesdeAlmacenId, model.HaciaAlmacenId);
            await LoadProductosAsync();
            return View(model);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var tr = new Transferencia
        {
            Fecha = DateTime.Now,
            DesdeAlmacenId = model.DesdeAlmacenId,
            HaciaAlmacenId = model.HaciaAlmacenId,
            UsuarioId = userId.Value,
            Documento = string.IsNullOrWhiteSpace(model.Documento) ? null : model.Documento.Trim(),
            Observacion = string.IsNullOrWhiteSpace(model.Observacion) ? null : model.Observacion.Trim()
        };

        _db.Transferencias.Add(tr);
        await _db.SaveChangesAsync();

        if (string.IsNullOrWhiteSpace(tr.Documento))
        {
            tr.Documento = $"T{tr.Id:000000}";
        }

        foreach (var item in model.Items)
        {
            var p = productos.First(x => x.Id == item.ProductoId);

            _db.DetallesTransferencia.Add(new DetalleTransferencia
            {
                TransferenciaId = tr.Id,
                ProductoId = p.Id,
                Cantidad = item.Cantidad
            });

            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                Fecha = tr.Fecha,
                Tipo = "Transferencia (Salida)",
                Documento = tr.Documento,
                ProductoId = p.Id,
                Cantidad = -item.Cantidad,
                UsuarioId = userId.Value
            });

            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                Fecha = tr.Fecha,
                Tipo = "Transferencia (Entrada)",
                Documento = tr.Documento,
                ProductoId = p.Id,
                Cantidad = item.Cantidad,
                UsuarioId = userId.Value
            });
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToAction(nameof(Detalle), new { id = tr.Id });
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Historial(DateTime? desde, DateTime? hasta)
    {
        ViewBag.ActiveModule = "Transferencias";
        ViewBag.ActiveSubModule = "Historial";

        var query = _db.Transferencias
            .AsNoTracking()
            .Include(x => x.DesdeAlmacen)
            .Include(x => x.HaciaAlmacen)
            .Include(x => x.Usuario)
            .AsQueryable();

        if (desde.HasValue)
        {
            query = query.Where(x => x.Fecha >= desde.Value);
        }

        if (hasta.HasValue)
        {
            var hastaInclusive = hasta.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.Fecha <= hastaInclusive);
        }

        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Take(200)
            .ToListAsync();

        ViewBag.Desde = desde;
        ViewBag.Hasta = hasta;
        return View(items);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        ViewBag.ActiveModule = "Transferencias";
        ViewBag.ActiveSubModule = "Historial";

        var tr = await _db.Transferencias
            .AsNoTracking()
            .Include(x => x.DesdeAlmacen)
            .Include(x => x.HaciaAlmacen)
            .Include(x => x.Usuario)
            .Include(x => x.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (tr is null) return NotFound();
        return View(tr);
    }

    private int? GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(str, out var id)) return id;
        return null;
    }

    private async Task LoadAlmacenesAsync(int? desdeSelected = null, int? haciaSelected = null)
    {
        var almacenes = await _db.Almacenes.AsNoTracking().Where(x => x.Activo).OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.AlmacenesDesde = new SelectList(almacenes, "Id", "Nombre", desdeSelected);
        ViewBag.AlmacenesHacia = new SelectList(almacenes, "Id", "Nombre", haciaSelected);
    }

    private async Task LoadProductosAsync()
    {
        var productos = await _db.Productos
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Select(x => new ProductoLookupViewModel
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Codigo = x.Codigo,
                Precio = x.Precio,
                Stock = x.Stock
            })
            .ToListAsync();

        ViewBag.Productos = productos;
    }
}

