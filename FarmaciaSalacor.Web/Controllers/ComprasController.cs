using System.Security.Claims;
using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using FarmaciaSalacor.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Controllers;

public class ComprasController : Controller
{
    private readonly AppDbContext _db;

    public ComprasController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    public IActionResult Index()
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Historial";
        return RedirectToAction(nameof(Historial));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Nueva()
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Nueva";

        await LoadProveedoresAsync();
        await LoadProductosAsync();

        var vm = new CompraNuevaViewModel
        {
            Items = new List<CompraNuevaItemViewModel> { new() { Cantidad = 1 } }
        };
        return View(vm);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(CompraNuevaViewModel model)
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Nueva";

        model.Items = model.Items
            .Where(x => x.ProductoId > 0 && x.Cantidad > 0)
            .ToList();

        if (model.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Agrega al menos un producto.");
        }

        if (!ModelState.IsValid)
        {
            await LoadProveedoresAsync(model.ProveedorId);
            await LoadProductosAsync();
            return View(model);
        }

        var userId = GetUserId();
        if (userId is null) return Forbid();

        var productoIds = model.Items.Select(x => x.ProductoId).Distinct().ToList();
        var productos = await _db.Productos.Where(x => productoIds.Contains(x.Id)).ToListAsync();

        foreach (var item in model.Items)
        {
            var p = productos.FirstOrDefault(x => x.Id == item.ProductoId);
            if (p is null)
            {
                ModelState.AddModelError(string.Empty, "Producto inválido.");
                await LoadProveedoresAsync(model.ProveedorId);
                await LoadProductosAsync();
                return View(model);
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var compra = new Compra
        {
            Fecha = DateTime.Now,
            NumeroDocumento = string.IsNullOrWhiteSpace(model.NumeroDocumento) ? null : model.NumeroDocumento.Trim(),
            ProveedorId = model.ProveedorId,
            UsuarioId = userId.Value,
            Total = 0
        };

        _db.Compras.Add(compra);
        await _db.SaveChangesAsync();

        if (string.IsNullOrWhiteSpace(compra.NumeroDocumento))
        {
            compra.NumeroDocumento = $"C{compra.Id:000000}";
        }

        decimal total = 0;
        foreach (var item in model.Items)
        {
            var p = productos.First(x => x.Id == item.ProductoId);
            var costo = item.CostoUnitario <= 0 ? p.Precio : item.CostoUnitario;
            var subtotal = Math.Round(item.Cantidad * costo, 2);
            total += subtotal;

            _db.DetallesCompra.Add(new DetalleCompra
            {
                CompraId = compra.Id,
                ProductoId = p.Id,
                Cantidad = item.Cantidad,
                CostoUnitario = costo,
                Subtotal = subtotal
            });

            p.Stock += item.Cantidad;
            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                Fecha = compra.Fecha,
                Tipo = "Compra",
                Documento = compra.NumeroDocumento,
                ProductoId = p.Id,
                Cantidad = item.Cantidad,
                UsuarioId = userId.Value
            });
        }

        compra.Total = Math.Round(total, 2);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToAction(nameof(Detalle), new { id = compra.Id });
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Historial(DateTime? desde, DateTime? hasta)
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Historial";

        var query = _db.Compras
            .AsNoTracking()
            .Include(x => x.Proveedor)
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
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Historial";

        var compra = await _db.Compras
            .AsNoTracking()
            .Include(x => x.Proveedor)
            .Include(x => x.Usuario)
            .Include(x => x.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (compra is null) return NotFound();
        return View(compra);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> Proveedores()
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Proveedores";

        var items = await _db.Proveedores.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return View(items);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public IActionResult ProveedorCreate()
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Proveedores";
        return View(new Proveedor());
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProveedorCreate(Proveedor model)
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Proveedores";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Proveedores.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Proveedores));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> ProveedorEdit(int id)
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Proveedores";

        var item = await _db.Proveedores.FindAsync(id);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProveedorEdit(int id, Proveedor model)
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Proveedores";

        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Proveedores.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Proveedores));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpGet]
    public async Task<IActionResult> ProveedorDelete(int id)
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Proveedores";

        var item = await _db.Proveedores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Almacen)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProveedorDeleteConfirmed(int id)
    {
        ViewBag.ActiveModule = "Compras";
        ViewBag.ActiveSubModule = "Proveedores";
        var item = await _db.Proveedores.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Proveedores));

        _db.Proveedores.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Proveedores));
    }

    private int? GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(str, out var id)) return id;
        return null;
    }

    private async Task LoadProveedoresAsync(int? selectedId = null)
    {
        var proveedores = await _db.Proveedores.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Proveedores = new SelectList(proveedores, "Id", "Nombre", selectedId);
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

