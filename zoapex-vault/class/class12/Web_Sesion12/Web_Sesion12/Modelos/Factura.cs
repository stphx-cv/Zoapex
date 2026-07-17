using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Sesion12.Modelos;

[Table("Tb_Factura")]
public class Factura
{
    [Key]
    [Column("Num_fac")]
    public string NumFac { get; set; } = "";

    [Column("Fec_fac")]
    public DateTime FecFac { get; set; }

    [Column("Fec_can")]
    public DateTime? FecCan { get; set; }

    [Column("Est_fac")]
    public int EstFac { get; set; }              // 1 pendiente, 2 cancelada, 3 anulada

    [Column("Cod_cli")]
    public string CodCli { get; set; } = "";

    [Column("Cod_ven")]
    public string CodVen { get; set; } = "";

    [Column("Por_igv")]
    public decimal PorIgv { get; set; }

    // Propiedades de navegacion
    public Cliente? Cliente { get; set; }
    public Vendedor? Vendedor { get; set; }
    public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();

    // Propiedad calculada (no esta en la BD): texto del estado para mostrar
    [NotMapped]
    public string EstadoTexto => EstFac switch
    {
        1 => "Pendiente",
        2 => "Cancelada",
        _ => "Anulada"
    };
}
