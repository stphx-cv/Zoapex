using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Sesion12.Modelos;

// Entidad que mapea la tabla Tb_Cliente. Generada al estilo "database-first"
// (en un proyecto real la crea el comando Scaffold-DbContext).
[Table("Tb_Cliente")]
public class Cliente
{
    [Key]
    [Column("Cod_cli")]
    public string CodCli { get; set; } = "";

    [Column("Raz_soc_cli")]
    public string RazSocCli { get; set; } = "";

    [Column("Ruc_cli")]
    public string? RucCli { get; set; }

    [Column("Dir_cli")]
    public string? DirCli { get; set; }

    [Column("Tel_cli")]
    public string? TelCli { get; set; }

    [Column("Est_cli")]
    public int EstCli { get; set; }

    // Propiedad de navegacion: las facturas de este cliente
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
