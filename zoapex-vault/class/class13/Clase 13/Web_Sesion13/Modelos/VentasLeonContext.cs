using Microsoft.EntityFrameworkCore;

namespace Web_Sesion13.Modelos;

// El DbContext es la sesion con la base de datos. Cada DbSet es una tabla
// consultable. Aqui se deja escrito a mano para fines didacticos.
public class VentasLeonContext : DbContext
{
    public VentasLeonContext(DbContextOptions<VentasLeonContext> options)
        : base(options) { }

    // Tablas de la Sesion 12
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Vendedor> Vendedores { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<DetalleFactura> DetalleFacturas { get; set; }
    public DbSet<FacturaResumen> FacturasResumen { get; set; }

    // Tablas nuevas de la Sesion 13
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Provincia> Provincias { get; set; }
    public DbSet<Distrito> Distritos { get; set; }

    // Conjunto sin clave que recibe el resultado del SP de paginacion
    public DbSet<FacturaPagina> FacturasPagina { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Clave compuesta del detalle (Num_fac + Cod_pro)
        modelBuilder.Entity<DetalleFactura>()
            .HasKey(d => new { d.NumFac, d.CodPro });

        // Entidades sin clave: solo reciben filas de un SP o consulta
        modelBuilder.Entity<FacturaResumen>().HasNoKey();
        modelBuilder.Entity<FacturaPagina>().HasNoKey();

        // Relaciones: se indica que las columnas existentes son las llaves foraneas,
        // para que EF Core no invente nombres de columna que no estan en la BD.
        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Cliente).WithMany(c => c.Facturas).HasForeignKey(f => f.CodCli);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Vendedor).WithMany(v => v.Facturas).HasForeignKey(f => f.CodVen);

        modelBuilder.Entity<DetalleFactura>()
            .HasOne(d => d.Factura).WithMany(f => f.Detalles).HasForeignKey(d => d.NumFac);
    }
}
