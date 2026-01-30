using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Documento { get; set; }

    [MaxLength(25)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }

    public bool Activo { get; set; } = true;
}
