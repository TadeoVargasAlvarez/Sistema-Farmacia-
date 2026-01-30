using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.Models;

public class MovimientoInventario
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(30)]
    public string Tipo { get; set; } = string.Empty; // Venta, Compra, Ajuste, Transferencia, etc.

    [MaxLength(60)]
    public string? Documento { get; set; }

    public int? ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public decimal Cantidad { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}
