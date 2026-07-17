namespace Web_Sesion13.Modelos;

// Entidad SIN clave (keyless): recibe el resultado del procedimiento
// almacenado usp_ListarFacturasClienteFechas (Ejemplo de la Sesion 12).
public class FacturaResumen
{
    public string Num_fac { get; set; } = "";
    public DateTime Fec_fac { get; set; }
    public string Estado { get; set; } = "";
    public decimal Total { get; set; }
}
