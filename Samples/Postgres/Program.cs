using Sixpence.ORM;
using Sixpence.ORM.Postgres;
using Sixpence.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// 日志记录加载成功
var logger = app.Services.GetRequiredService<ILogger<Program>>();

IServiceCollection services = new ServiceCollection();
services.AddServiceContainer(options => {
    options.Assembly.Add("Postgres");
});

app.UseORM(
    options => {
        options.EntityClassNameCase = NameCase.Pascal;
        options.LogOptions = new LogOptions()
        {
            LogDebug = message => logger.LogDebug(message),
            LogError = (message, exception) => logger.LogError(message, exception)
        };
    })
   .UsePostgres("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123123;", 20)
   .UseMigrateDB();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

