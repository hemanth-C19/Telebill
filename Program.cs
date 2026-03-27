using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Telebill.Repositories.Auth;
using Telebill.Services.Auth;
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
using Telebill.Services.Batch;
using Telebill.Repositories.Batch;
using Telebill.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

var tokenKey = builder.Configuration["Token"]
    ?? throw new InvalidOperationException("Token is not configured in appsettings.json.");
if (string.IsNullOrWhiteSpace(tokenKey))
    throw new InvalidOperationException("Token must be a non-empty secret in appsettings.json.");

builder.Services.AddAuthentication(options =>
    {  
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// MVC / Controllers — require authenticated user unless action/controller allows anonymous
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddFluentValidationAutoValidation();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

builder.Services.AddTransient<IClaimRepository, ClaimRepository>();
builder.Services.AddTransient<IClaimService, ClaimService>();
builder.Services.AddTransient<IClaimBuildService, ClaimBuildService>();
builder.Services.AddTransient<IClaimQueryService, ClaimQueryService>();
builder.Services.AddTransient<IClaimStatusService, ClaimStatusService>();
builder.Services.AddTransient<IClaimScrubService, ClaimScrubService>();
builder.Services.AddTransient<IClaimRuleService, ClaimRuleService>();
builder.Services.AddTransient<IClaimX12Service, ClaimX12Service>();

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

builder.Services.AddTransient<IBatchService, BatchService>();
builder.Services.AddTransient<IBatchRepository, BatchRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<Telebill.Validations.MasterData.PayerDtoValidator>();

// Add DbContext to DI (Scoped by default)
builder.Services.AddDbContext<TeleBillContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TelebillDb")));

var app = builder.Build();

app.UseGlobalExceptionMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuditLogger();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();