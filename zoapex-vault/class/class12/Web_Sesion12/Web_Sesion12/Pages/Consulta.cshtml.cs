using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web_Sesion12.Datos;
using Web_Sesion12.Modelos;

namespace Web_Sesion12.Pages;

// Ejemplo 1: consultas con LINQ sobre EF Core (deuda + facturas entre fechas).
public class ConsultaModel : PageModel
{
    private readonly VentasRepositorio _repo;

    // La capa de datos llega por inyeccion
    public ConsultaModel(VentasRepositorio repo)
    {
        _repo = repo;
    }

    [BindProperty]
    [Required(ErrorMessage = "Ingrese el codigo del cliente")]
    public string Codigo { get; set; } = "";

    [BindProperty, DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = new DateTime(2025, 1, 1);

    [BindProperty, DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = new DateTime(2025, 12, 31);

    public bool Consultado { get; set; }
    public decimal Deuda { get; set; }
    public List<Factura> Facturas { get; set; } = new();

    public void OnGet() { }

    public async Task OnPostAsync()
    {
        if (!ModelState.IsValid)
            return;

        // envio de datos a la capa de datos: se calcula la deuda y se listan las facturas
        Deuda = await _repo.CalcularDeudaClienteLinqAsync(Codigo);
        Facturas = await _repo.ListarFacturasLinqAsync(Codigo, FechaInicio, FechaFin);
        Consultado = true;
    }
}
