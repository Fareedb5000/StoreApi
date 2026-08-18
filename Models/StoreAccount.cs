namespace StoreAccount.Models;

using Microsoft.EntityFrameworkCore;

public class AccountRecord
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public int UserId { get; set; }
}

public class AccountStore : DbContext
{
    public DbSet<AccountRecord> Accounts { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=VALINDRA\SQLEXPRESS;Database=StoreAccountDb;Trusted_Connection=True;TrustServerCertificate=True;"
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccountRecord>()
            .Property(a => a.Balance)
            .HasPrecision(18, 2);
    }
}