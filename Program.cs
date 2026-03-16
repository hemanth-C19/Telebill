using Microsoft.Extensions.Options;
using Telebill.Models;
using Telebill.Repositories.Auth;
using Telebill.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Telebill.Services.MasterData;
using Telebill.Repositories.MasterData;
using Telebill.Repositories.Claims;

using Telebill.Repositories.PatientCoverage;
using Telebill.Services.PatientCoverage; 

using Telebill.Data;
using Repositories;
using Services;
using Telebill.Repositories.Attestations;
using Telebill.Repositories.ChargeLines;
using Telebill.Services.Attestations;
using Telebill.Services.ChargeLines;
using Telebill.Repositories.IdentityAccess;
using Telebill.Services.IdentityAccess;


var builder = WebApplication.CreateBuilder(args);


// MVC / Controllers
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

builder.Services.AddScoped<IEncounterRepository, EncounterRepository>();
builder.Services.AddScoped<IChargeLineRepository, ChargeLineRepository>();
builder.Services.AddScoped<IAttestationRepository, AttestationRepository>();

builder.Services.AddScoped<IEncounterService, EncounterService>();
builder.Services.AddScoped<IChargeLineService, ChargeLineService>();
builder.Services.AddScoped<IAttestationService, AttestationService>();
builder.Services.AddTransient<IPatientRepository, PatientRepository>();
builder.Services.AddTransient<IPatientService, PatientService>();

builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IClaimBuildService, ClaimBuildService>();
builder.Services.AddScoped<IClaimQueryService, ClaimQueryService>();
builder.Services.AddScoped<IClaimStatusService, ClaimStatusService>();
builder.Services.AddScoped<IClaimScrubService, ClaimScrubService>();
builder.Services.AddScoped<IClaimRuleService, ClaimRuleService>();
builder.Services.AddScoped<IClaimX12Service, ClaimX12Service>();

builder.Services.AddDbContext<TeleBillContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("TelebillDb"))
    ); 

// build app --> comes after service registration 

builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuditRepository, AuditRepository>();
builder.Services.AddTransient<IAuditService, AuditService>();

// Add DbContext to DI (Scoped by default)
builder.Services.AddDbContext<TeleBillContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TelebillDb")));

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline. --> Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();