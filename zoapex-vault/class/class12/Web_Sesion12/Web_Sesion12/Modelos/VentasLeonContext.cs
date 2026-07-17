using Microsoft.EntityFrameworkCore;

namespace Web_Sesion12.Modelos;

// El DbContext es la sesion con la base de datos. Cada DbSet es una tabla
// consultable. En database-first lo genera Scaffold-DbContext; aqui se deja
// escrito a mano para fines didacticos.
public class VentasLeonContext : DbContext
{
    public VentasLeonContext(DbContextOptions<VentasLeonContext> options)
        : base(options) { }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Vendedor> Vendedores { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<DetalleFactura> DetalleFacturas { get; set; }

    // Conjunto sin clave que recibe el resultado del procedimiento almacenado.
    public DbSet<FacturaResumen> FacturasResumen { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Clave compuesta del detalle (Num_fac + Cod_pro)
        modelBuilder.Entity<DetalleFactura>()
            .HasKey(d => new { d.NumFac, d.CodPro });

        // FacturaResumen no tiene clave: solo recibe filas de un SP/consulta
        modelBuilder.Entity<FacturaResumen>().HasNoKey();

        // Relaciones: se indica que las columnas existentes son las llaves foraneas
        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Cliente).WithMany(c => c.Facturas).HasForeignKey(f => f.CodCli);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Vendedor).WithMany(v => v.Facturas).HasForeignKey(f => f.CodVen);

        modelBuilder.Entity<DetalleFactura>()
            .HasOne(d => d.Factura).WithMany(f => f.Detalles).HasForeignKey(d => d.NumFac);
    }
}
