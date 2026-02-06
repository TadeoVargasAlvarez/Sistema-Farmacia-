using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaciaSalacor.Web.Models;

public class Producto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? NombreGenerico { get; set; }

    [MaxLength(100)]
    public string? FormaFarmaceutica { get; set; }

    [MaxLength(80)]
    public string? Concentracion { get; set; }

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int? MarcaId { get; set; }
    public Marca? Marca { get; set; }

    [MaxLength(120)]
    public string? Presentacion { get; set; }

    public decimal Stock { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    public DateOnly? Vencimiento { get; set; }

    public bool Activo { get; set; } = true;
}
