using Microsoft.EntityFrameworkCore;

namespace Api_Inmobiliaria.Models;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Propietario> Propietarios { get; set; } = null!;
    public DbSet<Inmueble> Inmuebles { get; set; } = null!;
    public DbSet<Contrato> Contratos { get; set; } = null!;
    public DbSet<Inquilino> Inquilinos { get; set; } = null!;
    public DbSet<Pago> Pagos { get; set; } = null!;
}
