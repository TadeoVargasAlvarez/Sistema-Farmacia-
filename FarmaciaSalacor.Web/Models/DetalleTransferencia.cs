namespace FarmaciaSalacor.Web.Models;

public class DetalleTransferencia
{
    public int Id { get; set; }

    public int TransferenciaId { get; set; }
    public Transferencia? Transferencia { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public decimal Cantidad { get; set; }
}
