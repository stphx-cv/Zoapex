namespace Web_Sesion13.Modelos;

// DTO para el grafico (Ejemplo 2): total facturado por anio.
// No es una tabla ni un DbSet; es solo el resultado de una consulta LINQ agrupada.
public class FacturacionAnual
{
    public int Anio { get; set; }
    public decimal Total { get; set; }
}
