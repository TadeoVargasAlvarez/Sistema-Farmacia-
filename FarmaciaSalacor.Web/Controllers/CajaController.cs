using FarmaciaSalacor.Web.Data;
using FarmaciaSalacor.Web.Models;
using FarmaciaSalacor.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSalacor.Web.Controllers;

public class CajaController : Controller
{
    private readonly AppDbContext _db;

    public CajaController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Cajero)]
    public async Task<IActionResult> Index(DateTime? fecha)
    {
        ViewBag.ActiveModule = "Caja";
        ViewBag.ActiveSubModule = "Resumen";

        var day = (fecha ?? DateTime.Today).Date;
        var next = day.AddDays(1);

        var ventasContadoDouble = await _db.Ventas
            .AsNoTracking()
            .Where(x => x.Fecha >= day && x.Fecha < next && !x.EsCredito)
            .Select(x => (double?)x.Total)
            .SumAsync() ?? 0d;

        var ventasCreditoDouble = await _db.Ventas
            .AsNoTracking()
            .Where(x => x.Fecha >= day && x.Fecha < next && x.EsCredito)
            .Select(x => (double?)x.Total)
            .SumAsync() ?? 0d;

        var cobrosCreditoDouble = await _db.PagosVenta
            .AsNoTracking()
            .Where(x => x.Fecha >= day && x.Fecha < next)
            .Select(x => (double?)x.Monto)
            .SumAsync() ?? 0d;

        var comprasDouble = await _db.Compras
            .AsNoTracking()
            .Where(x => x.Fecha >= day && x.Fecha < next)
            .Select(x => (double?)x.Total)
            .SumAsync() ?? 0d;

        var ventas = await _db.Ventas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Usuario)
            .Where(x => x.Fecha >= day && x.Fecha < next)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Take(50)
            .ToListAsync();

        var pagos = await _db.PagosVenta
            .AsNoTracking()
            .Include(x => x.Venta)
                .ThenInclude(v => v!.Cliente)
            .Include(x => x.Usuario)
            .Where(x => x.Fecha >= day && x.Fecha < next)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Take(50)
            .ToListAsync();

        var compras = await _db.Compras
            .AsNoTracking()
            .Include(x => x.Proveedor)
            .Include(x => x.Usuario)
            .Where(x => x.Fecha >= day && x.Fecha < next)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Take(50)
            .ToListAsync();

        var vm = new CajaDiaViewModel
        {
            Fecha = day,
            VentasContado = Convert.ToDecimal(ventasContadoDouble),
            VentasCredito = Convert.ToDecimal(ventasCreditoDouble),
            CobrosCredito = Convert.ToDecimal(cobrosCreditoDouble),
            Compras = Convert.ToDecimal(comprasDouble),
            Ventas = ventas,
            Pagos = pagos,
            ComprasLista = compras
        };

        return View(vm);
    }
}

