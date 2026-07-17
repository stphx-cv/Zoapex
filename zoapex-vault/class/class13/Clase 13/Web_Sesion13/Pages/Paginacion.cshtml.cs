using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_Sesion13.Datos;
using Web_Sesion13.Modelos;

namespace Web_Sesion13.Pages;

// Ejemplo 3: paginacion del listado de facturas, en dos modos (local y con SP).
public class PaginacionModel : PageModel
{
    private readonly VentasRepositorio _repo;

    public PaginacionModel(VentasRepositorio repo)
    {
        _repo = repo;
    }

    // El modo y la pagina llegan por la URL (?modo=sp&pagina=2)
    [BindProperty(SupportsGet = true)]
    public string Modo { get; set; } = "local";

    [BindProperty(SupportsGet = true)]
    public int Pagina { get; set; } = 1;

    public int Tam { get; } = 5;             // tamano de pagina
    public int Total { get; set; }           // total de facturas
    public int TotalPaginas { get; set; }    // cuantas paginas hay

    public List<Factura> FacturasLocal { get; set; } = new();
    public List<FacturaPagina> FacturasSp { get; set; } = new();

    public async Task OnGetAsync()
    {
        // se cuenta el total para saber cuantas paginas hay
        Total = await _repo.ContarFacturasAsync();
        TotalPaginas = (int)Math.Ceiling(Total / (double)Tam);
        if (TotalPaginas == 0) TotalPaginas = 1;

        // se corrige la pagina si viene fuera de rango
        if (Pagina < 1) Pagina = 1;
        if (Pagina > TotalPaginas) Pagina = TotalPaginas;

        // segun el modo elegido se llama a la capa de datos correspondiente
        if (Modo == "sp")
            FacturasSp = await _repo.ListarFacturasSpPaginacionAsync(Pagina, Tam);
        else
            FacturasLocal = await _repo.ListarFacturasLocalAsync(Pagina, Tam);
    }
}
