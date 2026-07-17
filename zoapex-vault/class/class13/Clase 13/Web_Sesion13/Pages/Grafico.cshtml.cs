using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_Sesion13.Datos;
using Web_Sesion13.Modelos;

namespace Web_Sesion13.Pages;

// Ejemplo 2: consulta de valor (facturacion anual) mostrada en tabla y en grafico.
public class GraficoModel : PageModel
{
    private readonly VentasRepositorio _repo;

    public GraficoModel(VentasRepositorio repo)
    {
        _repo = repo;
    }

    public List<FacturacionAnual> Facturacion { get; set; } = new();

    // listas planas que se serializan a JSON para Chart.js
    public List<string> Anios { get; set; } = new();
    public List<decimal> Totales { get; set; } = new();

    public async Task OnGetAsync()
    {
        // se pide el calculo a la capa de datos (LINQ con GroupBy)
        Facturacion = await _repo.FacturacionAnualAsync();

        // se arman las listas que necesita el grafico
        Anios = Facturacion.Select(f => f.Anio.ToString()).ToList();
        Totales = Facturacion.Select(f => f.Total).ToList();
    }
}
