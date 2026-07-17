using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Sesion13.Modelos;

[Table("Tb_Vendedor")]
public class Vendedor
{
    [Key]
    [Column("Cod_ven")]
    public string CodVen { get; set; } = "";

    [Column("Nom_ven")]
    public string NomVen { get; set; } = "";

    [Column("Ape_ven")]
    public string ApeVen { get; set; } = "";

    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
