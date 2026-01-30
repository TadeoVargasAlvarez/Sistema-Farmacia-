using FarmaciaSalacor.Web.Models;

namespace FarmaciaSalacor.Web.ViewModels;

public class EscritorioViewModel
{
    public DateTime Today { get; set; }

    public int ComprasDiaCount { get; set; }
    public decimal ComprasDiaTotal { get; set; }

    public int VentasDiaCount { get; set; }
    public decimal VentasDiaTotal { get; set; }

    public int CuentasPorCobrarCount { get; set; }
    public decimal CuentasPorCobrarSaldo { get; set; }

    public decimal StockTotal { get; set; }
    public int StockBajoCount { get; set; }

    public int VencidosCount { get; set; }
    public int PorVencerCount { get; set; }

    public decimal ComprasMesTotal { get; set; }
    public decimal VentasMesTotal { get; set; }

    public List<MovimientoInventario> MovimientosRecientes { get; set; } = new();
    public List<SerieDiaViewModel> Serie7Dias { get; set; } = new();
}

public class SerieDiaViewModel
{
    public DateTime Dia { get; set; }
    public decimal VentasTotal { get; set; }
    public decimal ComprasTotal { get; set; }
}
