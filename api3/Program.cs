using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using api3.Models;
using System.Runtime.Serialization;
using Microsoft.Extensions.Options;
using api3.Interface;
using api3.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<InterfaceEmployee, RepositoryEmployee>();
builder.Services.AddScoped<InterfaceStore, RepositoryStore>();
builder.Services.AddScoped<InterfaceInventory, RepositoryInventory>();
builder.Services.AddMemoryCache();
// Add services to the container.

builder.Services.AddDbContext<PgAdminContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));


});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
