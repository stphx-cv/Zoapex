using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Sesion13.Modelos;

// Entidad que mapea la tabla Tb_Proveedor (Ejemplo 1: CRUD).
[Table("Tb_Proveedor")]
public class Proveedor
{
    [Key]
    [Column("Cod_prv")]
    public string CodPrv { get; set; } = "";

    [Column("Raz_soc_prv")]
    public string RazSocPrv { get; set; } = "";

    [Column("Ruc_prv")]
    public string? RucPrv { get; set; }

    [Column("Dir_prv")]
    public string? DirPrv { get; set; }

    [Column("Cod_dep")]
    public string? CodDep { get; set; }

    [Column("Cod_prov")]
    public string? CodProv { get; set; }

    [Column("Cod_dist")]
    public string? CodDist { get; set; }

    [Column("Est_prv")]
    public int EstPrv { get; set; }   // 1 activo, 0 inactivo
}
