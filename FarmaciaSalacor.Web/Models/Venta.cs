using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaciaSalacor.Web.Models;

public class Venta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    [MaxLength(30)]
    public string? NumeroDocumento { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public bool EsCredito { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Pagado { get; set; }

    [NotMapped]
    public decimal Saldo => Total - Pagado;

    public List<DetalleVenta> Detalles { get; set; } = new();
}
