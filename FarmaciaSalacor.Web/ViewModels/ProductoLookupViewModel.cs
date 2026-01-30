namespace FarmaciaSalacor.Web.ViewModels;

public class ProductoLookupViewModel
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Codigo { get; set; }

    public decimal Precio { get; set; }

    public decimal Stock { get; set; }
}
