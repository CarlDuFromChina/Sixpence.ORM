using Microsoft.AspNetCore.Builder;
using Postgres.Entity;
using Sixpence.ORM;
using Sixpence.ORM.Postgres;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddTransient<IEntity, UserInfo>();
builder.Services.AddSorm(options =>
{
    options.NameCase = NameCase.Pascal;
    options.UsePostgres("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123123;", 20);
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() {  Title = "Postgres Demo 接口", Version = "v1"});
});

var app = builder.Build();

app.UseSorm(options =>
{
    options.EnableLogging = true;
    options.MigrateDb = true;
});

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.MapSwagger();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
});

app.Run();

