using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.ViewModels;

public class CompraNuevaViewModel
{
    public string? NumeroDocumento { get; set; }

    public int? ProveedorId { get; set; }

    public List<CompraNuevaItemViewModel> Items { get; set; } = new();
}

public class CompraNuevaItemViewModel
{
    [Required]
    public int ProductoId { get; set; }

    [Range(0.01, 9999999)]
    public decimal Cantidad { get; set; }

    [Range(0.00, 9999999)]
    public decimal CostoUnitario { get; set; }
}
