using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Sesion13.Modelos;

[Table("Tb_Departamento")]
public class Departamento
{
    [Key]
    [Column("Cod_dep")]
    public string CodDep { get; set; } = "";

    [Column("Nom_dep")]
    public string NomDep { get; set; } = "";
}
