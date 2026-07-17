using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Sesion13.Modelos;

[Table("Tb_Provincia")]
public class Provincia
{
    [Key]
    [Column("Cod_prov")]
    public string CodProv { get; set; } = "";

    [Column("Cod_dep")]
    public string CodDep { get; set; } = "";

    [Column("Nom_prov")]
    public string NomProv { get; set; } = "";
}
