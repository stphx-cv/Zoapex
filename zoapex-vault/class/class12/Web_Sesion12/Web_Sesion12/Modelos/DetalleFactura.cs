using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Sesion12.Modelos;

// La clave es compuesta (Num_fac + Cod_pro); se configura en el DbContext.
[Table("Tb_Detalle_Factura")]
public class DetalleFactura
{
    [Column("Num_fac")]
    public string NumFac { get; set; } = "";

    [Column("Cod_pro")]
    public string CodPro { get; set; } = "";

    [Column("Can_ven")]
    public int CanVen { get; set; }

    [Column("Pre_ven")]
    public decimal PreVen { get; set; }

    public Factura? Factura { get; set; }
}
