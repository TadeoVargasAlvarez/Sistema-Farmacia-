using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.ViewModels;

public class TransferenciaNuevaViewModel
{
    [Required]
    public int DesdeAlmacenId { get; set; }

    [Required]
    public int HaciaAlmacenId { get; set; }

    public string? Documento { get; set; }
    public string? Observacion { get; set; }

    public List<TransferenciaNuevaItemViewModel> Items { get; set; } = new();
}

public class TransferenciaNuevaItemViewModel
{
    [Required]
    public int ProductoId { get; set; }

    [Range(0.01, 9999999)]
    public decimal Cantidad { get; set; }
}
