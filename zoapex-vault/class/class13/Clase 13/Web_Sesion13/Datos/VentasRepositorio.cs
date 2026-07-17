using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Web_Sesion13.Modelos;

namespace Web_Sesion13.Datos;

// Capa de datos. Centraliza el acceso a la base de datos mediante EF Core.
// Las paginas NUNCA consultan la BD directamente: siempre llaman a estos metodos.
public class VentasRepositorio
{
    private readonly VentasLeonContext _ctx;

    public VentasRepositorio(VentasLeonContext ctx)
    {
        _ctx = ctx;
    }

    /* ================= SESION 12: CONSULTAS CON LINQ ================= */

    // Suma los importes de las facturas pendientes (estado 1) de un cliente.
    public async Task<decimal> CalcularDeudaClienteLinqAsync(string codigo)
    {
        decimal deuda = await (from f in _ctx.Facturas
                               join d in _ctx.DetalleFacturas on f.NumFac equals d.NumFac
                               where f.CodCli == codigo && f.EstFac == 1
                               select (decimal)(d.CanVen * d.PreVen)).SumAsync();
        return deuda;
    }

    // Lista las facturas de un cliente entre dos fechas (con su detalle).
    public async Task<List<Factura>> ListarFacturasLinqAsync(string codigo, DateTime ini, DateTime fin)
    {
        return await _ctx.Facturas
            .Include(f => f.Detalles)
            .Where(f => f.CodCli == codigo && f.FecFac >= ini && f.FecFac <= fin)
            .OrderBy(f => f.FecFac)
            .ToListAsync();
    }

    /* ============ SESION 12: PROCEDIMIENTOS ALMACENADOS ============ */

    public async Task<List<FacturaResumen>> ListarFacturasSpAsync(string codigo, DateTime ini, DateTime fin)
    {
        return await _ctx.FacturasResumen
            .FromSql($"EXEC usp_ListarFacturasClienteFechas {codigo}, {ini}, {fin}")
            .ToListAsync();
    }

    public async Task<decimal> CalcularDeudaClienteSpAsync(string codigo)
    {
        var pCodigo = new SqlParameter("@codigo", codigo);
        var pDeuda = new SqlParameter("@vdeuda", SqlDbType.Decimal)
        {
            Precision = 12,
            Scale = 2,
            Direction = ParameterDirection.Output
        };

        await _ctx.Database.ExecuteSqlRawAsync(
            "EXEC usp_DeudaCliente @codigo, @vdeuda OUTPUT", pCodigo, pDeuda);

        return pDeuda.Value == DBNull.Value ? 0 : (decimal)pDeuda.Value;
    }

    public async Task<Cliente?> BuscarClienteAsync(string codigo)
    {
        return await _ctx.Clientes.FirstOrDefaultAsync(c => c.CodCli == codigo);
    }

    /* ================= SESION 13 · EJEMPLO 1: PROVEEDORES (CRUD) ================= */

    // Lista proveedores; si el filtro viene vacio trae todos, si no filtra por razon social.
    public async Task<List<Proveedor>> ListarProveedoresAsync(string filtro)
    {
        filtro ??= "";
        return await _ctx.Proveedores
            .Where(p => filtro == "" || p.RazSocPrv.Contains(filtro))
            .OrderBy(p => p.RazSocPrv)
            .ToListAsync();
    }

    public async Task<Proveedor?> BuscarProveedorAsync(string codigo)
    {
        return await _ctx.Proveedores.FirstOrDefaultAsync(p => p.CodPrv == codigo);
    }

    public async Task InsertarProveedorAsync(Proveedor prov)
    {
        _ctx.Proveedores.Add(prov);
        await _ctx.SaveChangesAsync();
    }

    public async Task ActualizarProveedorAsync(Proveedor prov)
    {
        var p = await _ctx.Proveedores.FindAsync(prov.CodPrv);
        if (p == null) return;

        p.RazSocPrv = prov.RazSocPrv;
        p.RucPrv = prov.RucPrv;
        p.DirPrv = prov.DirPrv;
        p.CodDep = prov.CodDep;
        p.CodProv = prov.CodProv;
        p.CodDist = prov.CodDist;
        p.EstPrv = prov.EstPrv;

        await _ctx.SaveChangesAsync();   // EF Core detecta el cambio y hace el UPDATE
    }

    // Calcula el proximo codigo tipo P001, P002... a partir del ultimo registrado.
    public async Task<string> SiguienteCodigoProveedorAsync()
    {
        var ultimo = await _ctx.Proveedores
            .OrderByDescending(p => p.CodPrv)
            .Select(p => p.CodPrv)
            .FirstOrDefaultAsync();

        int n = 0;
        if (!string.IsNullOrEmpty(ultimo) && ultimo.Length > 1)
            int.TryParse(ultimo.Substring(1), out n);

        return "P" + (n + 1).ToString("000");
    }

    /* ================= SESION 13 · EJEMPLO 1: UBIGEO (cascada) ================= */

    public async Task<List<Departamento>> ListarDepartamentosAsync()
    {
        return await _ctx.Departamentos.OrderBy(d => d.NomDep).ToListAsync();
    }

    public async Task<List<Provincia>> ProvinciasPorDepartamentoAsync(string codDep)
    {
        return await _ctx.Provincias
            .Where(p => p.CodDep == codDep)
            .OrderBy(p => p.NomProv)
            .ToListAsync();
    }

    public async Task<List<Distrito>> DistritosPorProvinciaAsync(string codProv)
    {
        return await _ctx.Distritos
            .Where(d => d.CodProv == codProv)
            .OrderBy(d => d.NomDist)
            .ToListAsync();
    }

    /* ================= SESION 13 · EJEMPLO 2: CONSULTA DE VALOR ================= */

    // Total facturado por anio (no cuenta las anuladas). Alimenta la tabla y el grafico.
    public async Task<List<FacturacionAnual>> FacturacionAnualAsync()
    {
        return await (from f in _ctx.Facturas
                      join d in _ctx.DetalleFacturas on f.NumFac equals d.NumFac
                      where f.EstFac != 3
                      group (decimal)(d.CanVen * d.PreVen) by f.FecFac.Year into g
                      orderby g.Key
                      select new FacturacionAnual
                      {
                          Anio = g.Key,
                          Total = g.Sum()
                      }).ToListAsync();
    }

    /* ================= SESION 13 · EJEMPLO 3: PAGINACION ================= */

    // Total de facturas (para calcular cuantas paginas hay).
    public async Task<int> ContarFacturasAsync()
    {
        return await _ctx.Facturas.CountAsync();
    }

    // Paginacion LOCAL: EF Core traduce Skip/Take a OFFSET/FETCH en SQL Server.
    //public async Task<List<Factura>> ListarFacturasLocalAsync(int pagina, int tam)
    //{
    //    return await _ctx.Facturas
    //        .Include(f => f.Cliente)
    //        .Include(f => f.Vendedor)
    //        .Include(f => f.Detalles)
    //        .OrderBy(f => f.NumFac)
    //        .Skip((pagina - 1) * tam)   // salta las paginas anteriores
    //        .Take(tam)                  // toma solo los de esta pagina
    //        .ToListAsync();
    //}

    public async Task<List<Factura>> ListarFacturasLocalAsync(int pagina, int tam)
    {
        // Obtiene todas las facturas de la base de datos
        var lista = await _ctx.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Vendedor)
            .Include(f => f.Detalles)
            .OrderBy(f => f.NumFac)
            .ToListAsync();

        // La paginación se realiza en memoria con LINQ
        return lista
            .Skip((pagina - 1) * tam)
            .Take(tam)
            .ToList();
    }

    // Paginacion con SP: la base de datos devuelve solo la pagina pedida.
    public async Task<List<FacturaPagina>> ListarFacturasSpPaginacionAsync(int pagina, int tam)
    {
        return await _ctx.FacturasPagina
            .FromSql($"EXEC usp_ListarFacturas_Paginacion {pagina}, {tam}")
            .ToListAsync();
    }
}
