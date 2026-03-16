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
using Telebill.Repositories.Coding;
using Telebill.Services.Coding;
using Telebill.Repositories.PreCert;
using Telebill.Services.PreCert;


var builder = WebApplication.CreateBuilder(args);


// MVC / Controllers
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
builder.Services.AddScoped<IEncounterRepository, EncounterRepository>();
builder.Services.AddScoped<IChargeLineRepository, ChargeLineRepository>();
builder.Services.AddScoped<IAttestationRepository, AttestationRepository>();

builder.Services.AddTransient<IEncounterService, EncounterService>();
builder.Services.AddTransient<IChargeLineService, ChargeLineService>();
builder.Services.AddTransient<IAttestationService, AttestationService>();
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

builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IClaimBuildService, ClaimBuildService>();
builder.Services.AddScoped<IClaimQueryService, ClaimQueryService>();
builder.Services.AddScoped<IClaimStatusService, ClaimStatusService>();
builder.Services.AddScoped<IClaimScrubService, ClaimScrubService>();
builder.Services.AddScoped<IClaimRuleService, ClaimRuleService>();
builder.Services.AddScoped<IClaimX12Service, ClaimX12Service>();

builder.Services.AddTransient<IPreCertRepository, PreCertRepository>();
builder.Services.AddTransient<IPreCertService, PreCertService>();

builder.Services.AddTransient<IPreCertRepository, PreCertRepository>();
builder.Services.AddTransient<IPreCertService, PreCertService>();

// build app --> comes after service registration 

builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuditRepository, AuditRepository>();
builder.Services.AddTransient<IAuditService, AuditService>();
builder.Services.AddTransient<ICodingEncounterRepository, CodingEncounterRepository>();
builder.Services.AddTransient<IDiagnosisRepository, DiagnosisRepository>();
builder.Services.AddTransient<ICodingLockRepository, CodingLockRepository>();
builder.Services.AddTransient<IProviderCodingService, ProviderCodingService>();
builder.Services.AddTransient<ICoderWorklistService, CoderWorklistService>();
builder.Services.AddTransient<ICodingLockService, CodingLockService>();

// Add DbContext to DI (Scoped by default)
builder.Services.AddDbContext<TeleBillContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TelebillDb")));

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();