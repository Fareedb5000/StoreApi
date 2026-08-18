namespace UserSignUp.Models;
using Microsoft.EntityFrameworkCore;


public class UserSignUpRecord
{
    public int Id { get; set; }
    public  required string  UserName { get; set; }
    public  required string  PasswordHash { get; set; }
    public required string Email { get; set; }
    public  required string  FirstName { get; set; }
    public  required string  LastName { get; set; }
    
}
public class UserSignUpStore : DbContext
{
    public DbSet<UserSignUpRecord> UserSignUps { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=VALINDRA\SQLEXPRESS;Database=UserSignUpDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }
}