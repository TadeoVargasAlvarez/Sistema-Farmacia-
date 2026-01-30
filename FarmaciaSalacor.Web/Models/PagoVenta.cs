using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmaciaSalacor.Web.Models;

public class PagoVenta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [MaxLength(120)]
    public string? Observacion { get; set; }
}
