namespace Web_Sesion12.Modelos;

// Entidad SIN clave (keyless): no representa una tabla, sino el resultado
// del procedimiento almacenado usp_ListarFacturasClienteFechas.
// Sus propiedades coinciden con las columnas que devuelve el SP.
public class FacturaResumen
{
    public string Num_fac { get; set; } = "";
    public DateTime Fec_fac { get; set; }
    public string Estado { get; set; } = "";
    public decimal Total { get; set; }
}
