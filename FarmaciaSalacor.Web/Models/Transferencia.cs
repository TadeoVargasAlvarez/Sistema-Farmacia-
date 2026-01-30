using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.Models;

public class Transferencia
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public int DesdeAlmacenId { get; set; }
    public Almacen? DesdeAlmacen { get; set; }

    public int HaciaAlmacenId { get; set; }
    public Almacen? HaciaAlmacen { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [MaxLength(30)]
    public string? Documento { get; set; }

    [MaxLength(200)]
    public string? Observacion { get; set; }

    public List<DetalleTransferencia> Detalles { get; set; } = new();
}
