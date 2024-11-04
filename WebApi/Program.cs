using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("PostgreSQL")
            )
        );
        

        builder.Services.AddDomainDependencies();
        builder.Services.AddInfrastructureDependencies();

        builder.Services.AddControllers();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.MapControllers();

        app.Run();
    }
}