using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.Models;

public class Lote
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    [Required]
    [MaxLength(40)]
    public string NumeroLote { get; set; } = string.Empty;

    public DateOnly? Vencimiento { get; set; }

    public decimal Stock { get; set; }

    public bool Activo { get; set; } = true;
}
