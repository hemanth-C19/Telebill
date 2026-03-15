using Microsoft.EntityFrameworkCore;
using Telebill.Data;
using Repositories;
using Services;
using Telebill.Repositories.Attestations;
using Telebill.Repositories.ChargeLines;
using Telebill.Services.AR;
using Telebill.Services.Attestations;
using Telebill.Services.ChargeLines;
using Telebill.Services.Batch;
using Telebill.Repositories.Batch;
using Telebill.Repositories.AR;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories
builder.Services.AddScoped<IEncounterRepository, EncounterRepository>();
builder.Services.AddScoped<IChargeLineRepository, ChargeLineRepository>();
builder.Services.AddScoped<IAttestationRepository, AttestationRepository>();
builder.Services.AddScoped<IBatchRepository, BatchRepository>();
builder.Services.AddScoped<IArRepository,ArRepository>();


// Services
builder.Services.AddScoped<IEncounterService, EncounterService>();
builder.Services.AddScoped<IChargeLineService, ChargeLineService>();
builder.Services.AddScoped<IAttestationService, AttestationService>();
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<IArDashboardService,ArDashboardService>();
builder.Services.AddScoped<DenialService,DenialService>();
builder.Services.AddScoped<IUnderpaymentService,UnderpaymentService>();




builder.Services.AddDbContext<TeleBillContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")), 
    ServiceLifetime.Scoped
);    

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