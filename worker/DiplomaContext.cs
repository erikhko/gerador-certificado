using Microsoft.EntityFrameworkCore;

public class DiplomaContext : DbContext
{
    public DiplomaContext(DbContextOptions<DiplomaContext> options) : base(options) { }

    public DbSet<Diploma> Diplomas { get; set; }
}
