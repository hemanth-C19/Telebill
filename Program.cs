using Microsoft.EntityFrameworkCore;
using Telebill.Models;
using Telebill.Repositories.IdentityAccess;
using Telebill.Services.IdentityAccess;

var builder = WebApplication.CreateBuilder(args);


// MVC / Controllers
builder.Services.AddControllers();

// (Optional) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuditRepository, AuditRepository>();
builder.Services.AddTransient<IAuditService, AuditService>();

// Add DbContext to DI (Scoped by default)
builder.Services.AddDbContext<TeleBillContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TeleBillConnection")));

var app = builder.Build();

app.MapControllers();

// (Optional) Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();