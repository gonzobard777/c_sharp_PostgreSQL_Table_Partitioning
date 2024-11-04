using System.Data;
using System.Text;
using Domain.Contract;
using Domain.Entities;
using Domain.Services;
using Domain.ViewModels;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.ModelBinders;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PuansonController : ControllerBase
{
    private Random Random = new Random();
    private IPuansonService Service { get; }
    private MyDbContext DbContext { get; }

    public PuansonController(IPuansonService service, MyDbContext dbContext) : base()
    {
        Service = service;
        DbContext = dbContext;
    }

    private Task CreatePartition(DateTime date)
    {
        using (var command = DbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = $@"
CREATE TABLE puansons_{date.ToString("yyyyMMdd")} PARTITION OF ""Puansons""
   FOR VALUES FROM ('{date.ToString("yyyy-MM-dd")}') TO ('{date.AddDays(1).ToString("yyyy-MM-dd")}');
";
            command.CommandType = CommandType.Text;

            DbContext.Database.OpenConnection();

            using (var result = command.ExecuteReader())
            {
                return Task.CompletedTask;
            }
        }
    }

    [HttpGet("generate")]
    public async Task<ActionResult<string>> Generate()
    {
        var minutesInterval = 1;
        var startDate = DateTime.SpecifyKind(new DateTime(2024, 11, 3), DateTimeKind.Utc);
        var finalDate = DateTime.SpecifyKind(new DateTime(2024, 11, 6), DateTimeKind.Utc);

        var currentDate = startDate;
        await CreatePartition(currentDate);
        while (true)
        {
            if (currentDate >= finalDate)
                break;
            var nextDate = currentDate.AddMinutes(minutesInterval);
            if (
                currentDate.Day != nextDate.Day &&
                nextDate < finalDate
            )
            {
                await CreatePartition(nextDate);
            }

            await Service.CreateAsync(
                new Puanson
                {
                    Date = currentDate,
                    Data = """{"hello": """ + Random.Next() + "}"
                }
            );
            currentDate = nextDate;
        }

        return Ok("done");
    }

    [HttpGet]
    public async Task<PagedResult<Puanson>> List(
        [ModelBinder(BinderType = typeof(ListQueryParamsBinder), Name = "ListQueryParams")]
        ListQueryParams queryParams
    )
    {
        return await Service.List(queryParams);
    }
}