using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.ViewModels;

public class VentaNuevaViewModel
{
    public string? NumeroDocumento { get; set; }

    public int? ClienteId { get; set; }

    public bool EsCredito { get; set; }

    [Range(0, 9999999)]
    public decimal AbonoInicial { get; set; }

    public List<VentaNuevaItemViewModel> Items { get; set; } = new();
}

public class VentaNuevaItemViewModel
{
    [Required]
    public int ProductoId { get; set; }

    [Range(0.01, 9999999)]
    public decimal Cantidad { get; set; }

    [Range(0.00, 9999999)]
    public decimal PrecioUnitario { get; set; }
}
