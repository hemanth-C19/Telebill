using Microsoft.Extensions.Options;
using Telebill.Models;
using Telebill.Repositories.Auth;
using Telebill.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Telebill.Services.MasterData;
using Telebill.Repositories.MasterData;

using Telebill.Repositories.PatientCoverage;
using Telebill.Services.PatientCoverage; 

using Telebill.Data;
using Repositories;
using Services;
using Telebill.Repositories.Attestations;
using Telebill.Repositories.ChargeLines;
using Telebill.Services.Attestations;
using Telebill.Services.ChargeLines;
using Telebill.Services.Batch;
using Telebill.Repositories.Batch;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IAuthRepository, AuthRepository>();
builder.Services.AddTransient<IProviderService, ProviderService>();
builder.Services.AddTransient<IProviderRepository, ProviderRepository>();
builder.Services.AddTransient<IPayerService, PayerService>();
builder.Services.AddTransient<IPayerRepository, PayerRepository>();

builder.Services.AddTransient<IEncounterRepository, EncounterRepository>();
builder.Services.AddTransient<IChargeLineRepository, ChargeLineRepository>();
builder.Services.AddTransient<IAttestationRepository, AttestationRepository>();

builder.Services.AddTransient<IEncounterService, EncounterService>();
builder.Services.AddTransient<IChargeLineService, ChargeLineService>();
builder.Services.AddTransient<IAttestationService, AttestationService>();
builder.Services.AddTransient<IPatientRepository, PatientRepository>();
builder.Services.AddTransient<IPatientService, PatientService>();

builder.Services.AddTransient<IBatchService, BatchService>();
builder.Services.AddTransient<IBatchRepository, BatchRepository>();

builder.Services.AddDbContext<TeleBillContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("TelebillDb"))
    ); 

// build app --> comes after service registration 
var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline. --> Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();