using System.ComponentModel.DataAnnotations;
using FarmaciaSalacor.Web.Models;

namespace FarmaciaSalacor.Web.ViewModels;

public class UsuarioFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? NombreCompleto { get; set; }

    [Required]
    [MaxLength(20)]
    public string Rol { get; set; } = Roles.Cajero;

    public bool Activo { get; set; } = true;

    [DataType(DataType.Password)]
    public string? Password { get; set; }
}

public class UsuarioResetPasswordViewModel
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
