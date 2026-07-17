using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_Sesion13.Datos;

namespace Web_Sesion13.Pages;

// Sesion 12: AJAX moderno. Los handlers OnGet... devuelven JSON; el navegador
// los llama con fetch y actualiza solo una parte de la pagina, sin recargar.
public class AjaxModel : PageModel
{
    private readonly VentasRepositorio _repo;
    public AjaxModel(VentasRepositorio repo) { _repo = repo; }

    public void OnGet() { }

    // handler AJAX: devuelve el cliente y su deuda en JSON. ?handler=BuscarCliente
    public async Task<IActionResult> OnGetBuscarClienteAsync(string codigo)
    {
        var cliente = await _repo.BuscarClienteAsync(codigo);
        if (cliente == null)
            return new JsonResult(new { encontrado = false });

        decimal deuda = await _repo.CalcularDeudaClienteLinqAsync(codigo);
        return new JsonResult(new
        {
            encontrado = true,
            cliente.CodCli,
            cliente.RazSocCli,
            cliente.RucCli,
            cliente.DirCli,
            deuda
        });
    }

    // handler AJAX: devuelve las facturas del rango como una lista JSON. ?handler=Facturas
    public async Task<IActionResult> OnGetFacturasAsync(string codigo, DateTime ini, DateTime fin)
    {
        var facturas = await _repo.ListarFacturasLinqAsync(codigo, ini, fin);
        var datos = facturas.Select(f => new
        {
            numFac = f.NumFac,
            fecha = f.FecFac.ToString("dd/MM/yyyy"),
            estado = f.EstadoTexto,
            total = f.Detalles.Sum(d => d.CanVen * d.PreVen)
        });
        return new JsonResult(datos);
    }
}
