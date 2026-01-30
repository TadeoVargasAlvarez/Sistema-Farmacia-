using System.ComponentModel.DataAnnotations;

namespace FarmaciaSalacor.Web.Models;

public class Marca
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;
}
