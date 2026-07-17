using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Sesion13.Modelos;

[Table("Tb_Distrito")]
public class Distrito
{
    [Key]
    [Column("Cod_dist")]
    public string CodDist { get; set; } = "";

    [Column("Cod_prov")]
    public string CodProv { get; set; } = "";

    [Column("Nom_dist")]
    public string NomDist { get; set; } = "";
}
