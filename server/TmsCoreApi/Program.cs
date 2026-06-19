using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TmsCoreApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. SERVICE REGISTRATION LAYER (DI Container Workspace)
// =========================================================================
builder.Services.AddControllers(); 
builder.Services.AddProblemDetails(); 
builder.Services.AddOpenApi();

// Register In-Memory Storage Singletons with their abstract decoupling interfaces
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ITenderService, TenderService>();
builder.Services.AddSingleton<IBidService, BidService>();
builder.Services.AddSingleton<IEvaluationService, EvaluationService>();

var app = builder.Build();

// =========================================================================
// 2. MIDDLEWARE EXECUTION PIPELINE (Order Sensitive)
// =========================================================================

// A. Global Diagnostics & Tracking (Always Absolute Top)
app.Use(async (context, next) =>
{
    const string correlationIdHeader = "X-Correlation-ID";
    if (!context.Request.Headers.TryGetValue(correlationIdHeader, out var correlationId))
    {
        correlationId = Guid.NewGuid().ToString();
    }
    
    context.Response.Headers[correlationIdHeader] = correlationId;
    Activity.Current?.SetTag("CorrelationId", correlationId);
    
    await next();
});

// B. Global Error Catching Boundaries
app.UseExceptionHandler(); 
app.UseStatusCodePages();

// C. Production Security Enforcements (Before Routing maps endpoints)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// D. Routing Decider (Must occur BEFORE defining/mapping endpoint routes)
app.UseRouting();

// Future home for Security Middleware:
// app.UseAuthentication();
// app.UseAuthorization();

// E. Endpoint Execution Layer (Grouped cleanly at the absolute bottom)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Safely exposed at /scalar/v1 in development mode
}

app.MapControllers(); // Wires up your Tender, Bid, User, and Evaluation controllers

app.Run();