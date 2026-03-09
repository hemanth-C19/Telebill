using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Repositories;
using Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// DbContext
builder.Services.AddDbContext<TeleBillContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")), 
    ServiceLifetime.Scoped
);    


// Repositories
builder.Services.AddScoped<IEncounterRepository, EncounterRepository>();

// Services
builder.Services.AddScoped<IEncounterService, EncounterService>();


// build app --> comes after service registration 
var app = builder.Build();


// Configure the HTTP request pipeline. --> Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
app.Run();