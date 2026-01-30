using FarmaciaSalacor.Web.Models;

namespace FarmaciaSalacor.Web.ViewModels;

public class CajaDiaViewModel
{
    public DateTime Fecha { get; set; }

    public decimal VentasContado { get; set; }
    public decimal VentasCredito { get; set; }
    public decimal CobrosCredito { get; set; }
    public decimal Compras { get; set; }

    public decimal Ingresos => VentasContado + CobrosCredito;
    public decimal Egresos => Compras;
    public decimal Neto => Ingresos - Egresos;

    public List<Venta> Ventas { get; set; } = new();
    public List<PagoVenta> Pagos { get; set; } = new();
    public List<Compra> ComprasLista { get; set; } = new();
}
