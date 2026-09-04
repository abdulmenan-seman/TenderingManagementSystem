using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TmsApi.Api.Middleware;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Infrastructure;
using TmsApi.Infrastructure.Authorization;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller & Native OpenAPI Setup
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// 2. Global Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 3. Antiforgery (XSRF/CSRF Protection)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

// 4. DbContext Registration (PostgreSQL)
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TenderMsDb") 
            ?? throw new InvalidOperationException("Connection string 'TenderMsDb' not found."),
        b => b.MigrationsAssembly(typeof(TmsDbContext).Assembly.FullName)));

builder.Services.AddScoped<ITmsDbContext>(provider => provider.GetRequiredService<TmsDbContext>());

// 5. JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? "SuperSecretKeyForTmsApiProject2026!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// 6. Policy & Resource-Based Authorization Configuration
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireProcurementOfficer", policy => policy.RequireRole("Admin", "TenderOfficer"));
    options.AddPolicy("RequireEvaluator", policy => policy.RequireRole("Admin", "Evaluator"));
    options.AddPolicy("RequireBidderRole", policy => policy.RequireRole("Bidder"));
    
    // Resource Ownership Policy
    options.AddPolicy("MustOwnResource", policy => 
        policy.Requirements.Add(new ResourceOwnerRequirement()));
});

// Scoped to safely resolve DbContext inside the authorization check
builder.Services.AddScoped<IAuthorizationHandler, ResourceOwnerAuthorizationHandler>();

// 7. Rate Limiting Configuration
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("StrictPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 0;
    });
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Enforce OpenAPI 3.0 specification for UI component compatibility
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

// 8. CORS Policy Configuration
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// 9. Core Infrastructure & Application Registration (Includes Identity Core)
builder.Services.AddInfrastructureServices();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBidService, BidService>();
//builder.Services.AddScoped<ITenderService, TenderService>();
builder.Services.AddScoped<IEvaluationCriteriaService, EvaluationCriteriaService>();
builder.Services.AddScoped<IBidEvaluationService, BidEvaluationService>();

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(TmsApi.Application.Bids.Commands.SubmitBid.SubmitBidCommand).Assembly));

var app = builder.Build();


// 10. Middleware Pipeline Configuration
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Tendering Management System API")
        .WithTheme(ScalarTheme.Purple)
        .WithPreferredScheme("Bearer")
        .WithHttpBearerAuthentication(bearer =>
        {
            bearer.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzIiwiZW1haWwiOiJhbGVtQGdtYWlsLmNvbSIsInVuaXF1ZV9uYW1lIjoiQWxlbSBUZWtlbHUiLCJyb2xlIjoiVGVuZGVyT2ZmaWNlciIsIm5iZiI6MTc4NzczMzQzNSwiZXhwIjoxNzg3NzM0MzM1LCJpYXQiOjE3ODc3MzM0MzUsImlzcyI6IlRtc0FwaSIsImF1ZCI6IlRtc0NsaWVudCJ9.J9MVTIsvjt2sCKeP25rK5n8lZtAd-UCG6_si8yU1PVY";
        });
});
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("TmsClient");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// 11. Angular Anti-XSRF Token Cookie Injector Middleware
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true || context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false, // Readable by Angular HttpClient Cookie Anti-XSRF Interceptor
            Secure = !builder.Environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict
        });
    }
    await next(context);
});

app.MapControllers();

// 12. Automatic Database Seeding (Roles & System Administrator)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DatabaseSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding Identity roles and the admin user.");
    }
}

app.Run();