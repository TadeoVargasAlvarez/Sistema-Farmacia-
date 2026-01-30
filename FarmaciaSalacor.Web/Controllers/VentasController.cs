using System.Security.Claims;
using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using FarmaciaSalacor.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Controllers;

public class VentasController : Controller
{
    private readonly AppDbContext _db;

    public VentasController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "NuevaVenta";
        return RedirectToAction(nameof(Nueva));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public async Task<IActionResult> Nueva()
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "NuevaVenta";

        await LoadClientesAsync();
        await LoadProductosAsync();

        var vm = new VentaNuevaViewModel
        {
            Items = new List<VentaNuevaItemViewModel> { new() { Cantidad = 1 } }
        };
        return View(vm);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(VentaNuevaViewModel model)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "NuevaVenta";

        model.Items = model.Items
            .Where(x => x.ProductoId > 0 && x.Cantidad > 0)
            .ToList();

        if (model.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Agrega al menos un producto.");
        }

        if (model.EsCredito && model.ClienteId is null)
        {
            ModelState.AddModelError(nameof(model.ClienteId), "Para venta a crédito, selecciona un cliente.");
        }

        if (!ModelState.IsValid)
        {
            await LoadClientesAsync(model.ClienteId);
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
                await LoadClientesAsync(model.ClienteId);
                await LoadProductosAsync();
                return View(model);
            }

            var precio = item.PrecioUnitario;
            if (precio <= 0) precio = p.Precio;

            if (p.Stock < item.Cantidad)
            {
                ModelState.AddModelError(string.Empty, $"Stock insuficiente para {p.Nombre}. Disponible: {p.Stock:N2}");
                await LoadClientesAsync(model.ClienteId);
                await LoadProductosAsync();
                return View(model);
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var venta = new Venta
        {
            Fecha = DateTime.Now,
            NumeroDocumento = string.IsNullOrWhiteSpace(model.NumeroDocumento) ? null : model.NumeroDocumento.Trim(),
            ClienteId = model.ClienteId,
            UsuarioId = userId.Value,
            EsCredito = model.EsCredito,
            Total = 0,
            Pagado = 0
        };

        _db.Ventas.Add(venta);
        await _db.SaveChangesAsync();

        if (string.IsNullOrWhiteSpace(venta.NumeroDocumento))
        {
            venta.NumeroDocumento = $"V{venta.Id:000000}";
        }

        decimal total = 0;
        foreach (var item in model.Items)
        {
            var p = productos.First(x => x.Id == item.ProductoId);
            var precio = item.PrecioUnitario <= 0 ? p.Precio : item.PrecioUnitario;
            var subtotal = Math.Round(item.Cantidad * precio, 2);
            total += subtotal;

            _db.DetallesVenta.Add(new DetalleVenta
            {
                VentaId = venta.Id,
                ProductoId = p.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = precio,
                Subtotal = subtotal
            });

            p.Stock -= item.Cantidad;
            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                Fecha = venta.Fecha,
                Tipo = "Venta",
                Documento = venta.NumeroDocumento,
                ProductoId = p.Id,
                Cantidad = -item.Cantidad,
                UsuarioId = userId.Value
            });
        }

        venta.Total = Math.Round(total, 2);

        if (!venta.EsCredito)
        {
            venta.Pagado = venta.Total;
        }
        else
        {
            var abono = Math.Max(0, Math.Round(model.AbonoInicial, 2));
            if (abono > 0)
            {
                if (abono > venta.Total) abono = venta.Total;
                venta.Pagado = abono;
                _db.PagosVenta.Add(new PagoVenta
                {
                    VentaId = venta.Id,
                    Fecha = DateTime.Now,
                    Monto = abono,
                    UsuarioId = userId.Value,
                    Observacion = "Abono inicial"
                });
            }
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return RedirectToAction(nameof(Detalle), new { id = venta.Id });
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public async Task<IActionResult> Historial(DateTime? desde, DateTime? hasta)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Historial";

        var query = _db.Ventas
            .AsNoTracking()
            .Include(x => x.Cliente)
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

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Historial";

        var venta = await _db.Ventas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Usuario)
            .Include(x => x.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (venta is null) return NotFound();

        var pagos = await _db.PagosVenta
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Where(x => x.VentaId == id)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync();

        ViewBag.Pagos = pagos;
        return View(venta);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public async Task<IActionResult> Clientes()
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Clientes";

        var items = await _db.Clientes.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        return View(items);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public IActionResult ClienteCreate()
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Clientes";
        return View(new Cliente());
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClienteCreate(Cliente model)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Clientes";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Clientes.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Clientes));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public async Task<IActionResult> ClienteEdit(int id)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Clientes";

        var item = await _db.Clientes.FindAsync(id);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClienteEdit(int id, Cliente model)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Clientes";

        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.Clientes.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Clientes));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public async Task<IActionResult> ClienteDelete(int id)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Clientes";

        var item = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClienteDeleteConfirmed(int id)
    {
        var item = await _db.Clientes.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Clientes));

        _db.Clientes.Remove(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Clientes));
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public async Task<IActionResult> Cxc()
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Cxc";

        var items = await _db.Ventas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Where(x => x.EsCredito && x.Pagado < x.Total)
            .OrderByDescending(x => x.Fecha)
            .Take(200)
            .ToListAsync();

        return View(items);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpGet]
    public async Task<IActionResult> Abonar(int id)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Cxc";

        var venta = await _db.Ventas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (venta is null) return NotFound();
        return View(venta);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abonar(int id, decimal monto, string? observacion)
    {
        ViewBag.ActiveModule = "Ventas";
        ViewBag.ActiveSubModule = "Cxc";

        var userId = GetUserId();
        if (userId is null) return Forbid();

        var venta = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == id);
        if (venta is null) return NotFound();

        var m = Math.Round(monto, 2);
        if (m <= 0)
        {
            ModelState.AddModelError(string.Empty, "El monto debe ser mayor que cero.");
            return View(venta);
        }

        var saldo = venta.Total - venta.Pagado;
        if (m > saldo) m = saldo;

        venta.Pagado = Math.Round(venta.Pagado + m, 2);
        _db.PagosVenta.Add(new PagoVenta
        {
            VentaId = venta.Id,
            Fecha = DateTime.Now,
            Monto = m,
            UsuarioId = userId.Value,
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim()
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Cxc));
    }

    private int? GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(str, out var id)) return id;
        return null;
    }

    private async Task LoadClientesAsync(int? selectedId = null)
    {
        var clientes = await _db.Clientes.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();
        ViewBag.Clientes = new SelectList(clientes, "Id", "Nombre", selectedId);
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
