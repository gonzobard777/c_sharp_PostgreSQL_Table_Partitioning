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
    private string ContentStr;
    private string PrevDateStr = "";
    private string PrevSql = "";
    private IPuansonService Service { get; }
    private MyDbContext DbContext { get; }

    public PuansonController(IPuansonService service, MyDbContext dbContext) : base()
    {
        Service = service;
        DbContext = dbContext;
        ContentStr = System.IO.File.ReadAllText(@"D:\puanson.json");
    }

    private Task CreatePartition(DateTime date)
    {
        try
        {
            return ExecuteRawSql($@"
CREATE TABLE puansons_{date.ToString("yyyyMMdd")} PARTITION OF ""Puansons""
   FOR VALUES FROM ('{date.ToString("yyyy-MM-dd")}') TO ('{date.AddDays(1).ToString("yyyy-MM-dd")}');
");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Task.CompletedTask;
        }
    }

    private Task ExecuteRawSql(string sql)
    {
        using (var command = DbContext.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = sql;
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
        Console.WriteLine("Generating Puansons...");
        Console.WriteLine($@"start: {DateTime.Now}");
        var step = 1;
        var startDate = DateTime.SpecifyKind(DateTime.Parse("2024-05-16T00:00:00"), DateTimeKind.Utc);
        var finalDate = DateTime.SpecifyKind(DateTime.Parse("2024-10-16T00:00:00"), DateTimeKind.Utc);

        var currentDate = startDate;
        await CreatePartition(currentDate);
        Console.WriteLine($@"partition: {startDate.ToString("yyyyMMdd")} at {DateTime.Now}");
        while (true)
        {
            if (currentDate >= finalDate)
                break;

            var nextDate = currentDate.AddHours(step);

            if (
                currentDate.Day != nextDate.Day &&
                nextDate < finalDate
            )
            {
                await CreatePartition(nextDate);
                Console.WriteLine($@"partition: {nextDate.ToString("yyyyMMdd")} at {DateTime.Now}");
            }

            // 17000 пуансонов в час
            // for (int i = 0; i < 17000; i++)
            //     await Service.CreateAsync(
            //         new Puanson
            //         {
            //             Date = currentDate,
            //             Data = """{"hello": """ + Random.Next() + "}"
            //         }
            //     );
            await InsertRawSql(currentDate);

            currentDate = nextDate;
        }

        Console.WriteLine($@"finish: {DateTime.Now}");
        return Ok("done");
    }

    private Task InsertRawSql(DateTime date)
    {
        var sql = "";
        var dateStr = date.ToString("yyyyMMdd HH:mm:ss.000000");

        if (PrevDateStr == "" || PrevSql == "")
        {
            var postfix = $@" '{dateStr}', '{ContentStr}')";
            // var value = $@"(DEFAULT, '{dateStr}', '{{""hello"": {Random.Next()}}}')";
            sql = """INSERT INTO "Puansons" ("Id", "Date", "Data") VALUES """;
            var len = 17_000;
            // var len = 1;
            for (int i = 1; i <= len; i++)
            {
                // sql += $@"({i}, '{dateStr}', '{{""hello"": {Random.Next()}}}')";
                sql += $@"({i}, {postfix}";
                if (i != len)
                {
                    sql += ',';
                }
            }
        }
        else
        {
            sql = PrevSql.Replace(PrevDateStr, dateStr);
        }

        PrevDateStr = dateStr;
        PrevSql = sql;
        return ExecuteRawSql(sql);
    }

    private async Task GenerateByMinutes()
    {
        var minutesInterval = 1;
        var startDate = DateTime.SpecifyKind(new DateTime(2024, 11, 3), DateTimeKind.Utc);
        var finalDate = DateTime.SpecifyKind(new DateTime(2024, 11, 5), DateTimeKind.Utc);

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