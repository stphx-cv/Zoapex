using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Web_Sesion12.Datos;
using Web_Sesion12.Modelos;

namespace Web_Sesion12.Pages;

// Ejemplo 2: ejecutar procedimientos almacenados desde EF Core.
public class ProcedimientoModel : PageModel
{
    private readonly VentasRepositorio _repo;

    public ProcedimientoModel(VentasRepositorio repo)
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
    public List<FacturaResumen> Facturas { get; set; } = new();

    public void OnGet() { }

    public async Task OnPostAsync()
    {
        if (!ModelState.IsValid)
            return;

        // se invocan los procedimientos almacenados a traves de la capa de datos
        Deuda = await _repo.CalcularDeudaClienteSpAsync(Codigo);
        Facturas = await _repo.ListarFacturasSpAsync(Codigo, FechaInicio, FechaFin);
        Consultado = true;
    }
}
