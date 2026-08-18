namespace InventoryItem.Models;

using Microsoft.EntityFrameworkCore;


public class InventoryRecord
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public int Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int UserId { get; set; }
}


public class InventoryItems : DbContext
{

    public DbSet<InventoryRecord> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(@"Server=VALINDRA\SQLEXPRESS;Database=InventoryDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }
}
