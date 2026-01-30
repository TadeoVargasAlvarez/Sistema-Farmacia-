using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Rol { get; set; } = Roles.Cajero;

    [MaxLength(120)]
    public string? NombreCompleto { get; set; }

    public bool Activo { get; set; } = true;
}
