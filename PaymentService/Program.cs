using MassTransit;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentService.Consumers;
using PaymentService.Data;
using PaymentService.Interfaces;
using PaymentService.Repository;
using PaymentService.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================== CONTROLLERS & SWAGGER ==================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PaymentService API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ================== BANCO ==================
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService.Services.PaymentService>();

// ================== CORS ==================
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ================== APPLICATION INSIGHTS ==================
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, CloudRoleTelemetryInitializer>();

// ================== AUTH / JWT ==================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

builder.Services.AddAuthorization();

// ================== HTTP CLIENTS ==================
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();

//builder.Services.AddHttpClient<UserClient>(c =>
//{
//    c.BaseAddress = new Uri(builder.Configuration["USERS_URL"] ?? "https://localhost:7126");
//}).AddHttpMessageHandler<BearerTokenHandler>();

//builder.Services.AddHttpClient<GameClient>(c =>
//{
//    c.BaseAddress = new Uri(builder.Configuration["GAMES_URL"] ?? "https://localhost:7093");
//}).AddHttpMessageHandler<BearerTokenHandler>();

builder.Services.AddHttpClient<UserClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["USERS_URL"] ?? "https://localhost:7126");
    c.DefaultRequestHeaders.Add("X-Internal-Call", "true");
}).AddHttpMessageHandler<BearerTokenHandler>();

builder.Services.AddHttpClient<GameClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["GAMES_URL"] ?? "https://localhost:7093");
    c.DefaultRequestHeaders.Add("X-Internal-Call", "true");
}).AddHttpMessageHandler<BearerTokenHandler>();

// ================== MASSTRANSIT / RABBITMQ ==================
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<GamePurchasedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        cfg.ConfigureEndpoints(context);

        cfg.PrefetchCount = 10;
    });
});

// ================== HEALTHCHECK (somente registro) ==================
builder.Services.AddHealthChecks();

var app = builder.Build();

//para aplicar as migrations na primeira vez que subir o container do Docker

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    int retries = 0;
    const int maxRetries = 10;

    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            retries++;
            if (retries >= maxRetries)
                throw;
            Console.WriteLine($"Banco ainda não pronto... tentando novamente ({retries}/{maxRetries})");
            Thread.Sleep(3000);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Telemetry initializer
public class CloudRoleTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "PaymentService";
    }
}