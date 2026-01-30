using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.Models;

public class Almacen
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
