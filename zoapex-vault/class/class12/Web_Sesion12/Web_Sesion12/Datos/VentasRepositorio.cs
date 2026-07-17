using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Web_Sesion12.Modelos;

namespace Web_Sesion12.Datos;

// Capa de datos. Centraliza el acceso a la base de datos mediante EF Core.
// Las paginas NUNCA consultan la BD directamente: siempre llaman a estos metodos.
public class VentasRepositorio
{
    private readonly VentasLeonContext _ctx;

    // El DbContext llega por inyeccion (registrado en Program.cs)
    public VentasRepositorio(VentasLeonContext ctx)
    {
        _ctx = ctx;
    }

    /* ================= EJEMPLO 1: CONSULTAS CON LINQ ================= */

    // Suma los importes de las facturas pendientes (estado 1) de un cliente.
    // Consulta a la base de datos via LINQ to Entities (join + Sum).
    public async Task<decimal> CalcularDeudaClienteLinqAsync(string codigo)
    {
        decimal deuda = await (from f in _ctx.Facturas
                               join d in _ctx.DetalleFacturas on f.NumFac equals d.NumFac
                               where f.CodCli == codigo && f.EstFac == 1
                               select (decimal)(d.CanVen * d.PreVen)).SumAsync();
        return deuda;
    }

    // Lista las facturas de un cliente entre dos fechas (sintaxis de metodo).
    // Include trae el detalle para poder mostrar el total de cada factura.
    public async Task<List<Factura>> ListarFacturasLinqAsync(string codigo, DateTime ini, DateTime fin)
    {
        return await _ctx.Facturas
            .Include(f => f.Detalles)
            .Where(f => f.CodCli == codigo && f.FecFac >= ini && f.FecFac <= fin)
            .OrderBy(f => f.FecFac)
            .ToListAsync();
    }

    /* ========= EJEMPLO 2: PROCEDIMIENTOS ALMACENADOS EN EF CORE ========= */

    // Ejecuta el SP usp_ListarFacturasClienteFechas y mapea el resultado
    // a la entidad sin clave FacturaResumen. La interpolacion se convierte
    // en parametros, por lo que es segura frente a inyeccion SQL.
    public async Task<List<FacturaResumen>> ListarFacturasSpAsync(string codigo, DateTime ini, DateTime fin)
    {
        return await _ctx.FacturasResumen
            .FromSql($"EXEC usp_ListarFacturasClienteFechas {codigo}, {ini}, {fin}")
            .ToListAsync();
    }

    // Ejecuta el SP usp_DeudaCliente, que devuelve la deuda por un parametro
    // de SALIDA (OUTPUT). Asi se leen los parametros de salida en EF Core.
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

    /* ================= EJEMPLO 3: APOYO PARA EL AJAX ================= */

    // Busca un cliente por su codigo. Lo usa el handler AJAX para refrescar
    // el panel sin recargar la pagina.
    public async Task<Cliente?> BuscarClienteAsync(string codigo)
    {
        return await _ctx.Clientes
            .FirstOrDefaultAsync(c => c.CodCli == codigo);
    }
}
