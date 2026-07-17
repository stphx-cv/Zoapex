using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_Sesion13.Datos;
using Web_Sesion13.Modelos;

namespace Web_Sesion13.Pages;

// Ejemplo 1: CRUD de proveedores. Todos los handlers OnGet... devuelven JSON
// y el guardado se hace por POST con fetch. La vista nunca toca la BD.
public class ProveedoresModel : PageModel
{
    private readonly VentasRepositorio _repo;
    public ProveedoresModel(VentasRepositorio repo) { _repo = repo; }

    public void OnGet() { }

    // ?handler=Listar  -> lista de proveedores (opcionalmente filtrada)
    public async Task<IActionResult> OnGetListarAsync(string? filtro)
    {
        var lista = await _repo.ListarProveedoresAsync(filtro ?? "");
        var datos = lista.Select(p => new
        {
            p.CodPrv, p.RazSocPrv, p.RucPrv, p.EstPrv
        });
        return new JsonResult(datos);
    }

    // ?handler=Obtener -> un proveedor para cargar el modal de edicion
    public async Task<IActionResult> OnGetObtenerAsync(string codigo)
    {
        var p = await _repo.BuscarProveedorAsync(codigo);
        if (p == null) return new JsonResult(new { });
        return new JsonResult(new
        {
            p.CodPrv, p.RazSocPrv, p.RucPrv, p.DirPrv,
            p.CodDep, p.CodProv, p.CodDist, p.EstPrv
        });
    }

    // ?handler=NuevoCodigo -> proximo codigo correlativo (P001, P002...)
    public async Task<IActionResult> OnGetNuevoCodigoAsync()
    {
        return new JsonResult(new { codigo = await _repo.SiguienteCodigoProveedorAsync() });
    }

    // ?handler=Departamentos -> combo de departamentos { valor, texto }
    public async Task<IActionResult> OnGetDepartamentosAsync()
    {
        var lista = await _repo.ListarDepartamentosAsync();
        return new JsonResult(lista.Select(d => new { valor = d.CodDep, texto = d.NomDep }));
    }

    // ?handler=Provincias -> provincias del departamento elegido
    public async Task<IActionResult> OnGetProvinciasAsync(string dep)
    {
        var lista = await _repo.ProvinciasPorDepartamentoAsync(dep);
        return new JsonResult(lista.Select(p => new { valor = p.CodProv, texto = p.NomProv }));
    }

    // ?handler=Distritos -> distritos de la provincia elegida
    public async Task<IActionResult> OnGetDistritosAsync(string prov)
    {
        var lista = await _repo.DistritosPorProvinciaAsync(prov);
        return new JsonResult(lista.Select(d => new { valor = d.CodDist, texto = d.NomDist }));
    }

    // POST ?handler=Guardar -> inserta o actualiza segun exista el codigo
    public async Task<IActionResult> OnPostGuardarAsync([FromBody] Proveedor prov)
    {
        if (prov == null || string.IsNullOrWhiteSpace(prov.RazSocPrv))
            return new JsonResult(new { ok = false, msg = "Datos incompletos." });

        var existe = await _repo.BuscarProveedorAsync(prov.CodPrv);
        if (existe == null)
            await _repo.InsertarProveedorAsync(prov);
        else
            await _repo.ActualizarProveedorAsync(prov);

        return new JsonResult(new { ok = true });
    }
}
