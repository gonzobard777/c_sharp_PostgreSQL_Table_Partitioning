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
    private string PrevSql = "";
    private string PrevDateStr = "";

    private IPuansonService Service { get; }
    private MyDbContext DbContext { get; }

    public PuansonController(IPuansonService service, MyDbContext dbContext) : base()
    {
        Service = service;
        DbContext = dbContext;
    }


    [HttpGet("generate")]
    public async Task<ActionResult<string>> Generate()
    {
        var step = 1;
        var startDate = DateTime.SpecifyKind(DateTime.Parse("2024-04-16T00:00:00"), DateTimeKind.Utc);
        var finalDate = DateTime.SpecifyKind(DateTime.Parse("2024-05-16T00:00:00"), DateTimeKind.Utc);

        var currentDate = startDate;
        await CreatePartition(currentDate);
        Console.WriteLine("Generating Puansons...");
        Console.WriteLine($@"start: {DateTime.Now}");
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

            await InsertGeneratedToDB(currentDate);

            currentDate = nextDate;
        }

        Console.WriteLine($@"finish: {DateTime.Now}");
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


    #region Utils

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

    /// <summary>
    /// Создать партицию по Дате.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
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

    private Task InsertGeneratedToDB(DateTime date)
    {
        var sql = "";
        var dateStr = date.ToString("yyyyMMdd HH:mm:ss.000000");

        // Сгенерировать день, если еще не сгенерирован, вида:
        //   INSERT INTO "Puansons" ("Id", "Date", "Data") VALUES 
        //     (1, '2024-11-03 00:00:00.000000', 'ТУТ_СОДЕРЖИМОЕ_ОДНОГО_ПУАНСОНА')
        //     (2, '2024-11-03 00:00:00.000000', 'ТУТ_СОДЕРЖИМОЕ_ОДНОГО_ПУАНСОНА')
        //     ...
        //     (16999, '2024-11-03 23:00:00.000000', 'ТУТ_СОДЕРЖИМОЕ_ОДНОГО_ПУАНСОНА')
        //     (17000, '2024-11-03 23:00:00.000000', 'ТУТ_СОДЕРЖИМОЕ_ОДНОГО_ПУАНСОНА')
        if (PrevDateStr == "" || PrevSql == "")
        {
            sql = """INSERT INTO "Puansons" ("Id", "Date", "Data") VALUES """;

            var contentStr = System.IO.File.ReadAllText(@"D:\puanson.json");
            var postfix = $@" '{dateStr}', '{contentStr}')";
            // var value = $@"(DEFAULT, '{dateStr}', '{{""hello"": {Random.Next()}}}')";

            var len = 17_000;
            for (int i = 1; i <= len; i++)
            {
                // sql += $@"({i}, '{dateStr}', '{{""hello"": {Random.Next()}}}')";
                sql += $@"({i}, {postfix}";
                if (i != len)
                    sql += ',';
            }
        }
        // Если день сгененирован, тогда взять и заменить дату.
        // Работает намнооого быстрее, чем заново генерировать день.
        else
            sql = PrevSql.Replace(PrevDateStr, dateStr);

        PrevDateStr = dateStr;
        PrevSql = sql;
        return ExecuteRawSql(sql);
    }

    private async Task GenerateByMinutes()
    {
        var random = new Random();
        var step = 1;
        var startDate = DateTime.SpecifyKind(new DateTime(2024, 11, 3), DateTimeKind.Utc);
        var finalDate = DateTime.SpecifyKind(new DateTime(2024, 11, 5), DateTimeKind.Utc);

        var currentDate = startDate;
        await CreatePartition(currentDate);
        while (true)
        {
            if (currentDate >= finalDate)
                break;
            var nextDate = currentDate.AddMinutes(step);
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
                    Data = """{"hello": """ + random.Next() + "}"
                }
            );
            currentDate = nextDate;
        }
    }

    #endregion Utils
}