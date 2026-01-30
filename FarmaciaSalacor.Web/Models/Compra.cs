using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaciaSalacor.Web.Models;

public class Compra
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [MaxLength(30)]
    public string? NumeroDocumento { get; set; }

    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public List<DetalleCompra> Detalles { get; set; } = new();
}
