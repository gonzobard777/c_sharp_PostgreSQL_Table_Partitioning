using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class MyDbContext : DbContext
{
    public MyDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }
    
    public DbSet<Puanson> Puansons { get; set; }
}