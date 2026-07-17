namespace Web_Sesion13.Modelos;

// Entidad SIN clave (keyless): recibe las filas de la vista vw_VistaFacturas
// que devuelve el SP de paginacion usp_ListarFacturas_Paginacion (Ejemplo 3).
public class FacturaPagina
{
    public string Num_fac { get; set; } = "";
    public DateTime Fec_fac { get; set; }
    public string Cliente { get; set; } = "";
    public string Vendedor { get; set; } = "";
    public string Estado { get; set; } = "";
    public decimal Total { get; set; }
}
