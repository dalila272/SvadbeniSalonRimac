using SvadbeniSalon.Common.Services;
using SvadbeniSalon.Common.Services.CryptoService;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Services;
using SvadbeniSalon.Services.Database;
using SvadbeniSalon.Services.Validators;
using SvadbeniSalon.WebAPI.Filters;
using SvadbeniSalon.WebAPI.Hubs;
using SvadbeniSalon.WebAPI.Services;
using SvadbeniSalon.WebAPI.Services.AccessManager;
using EasyNetQ;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

EnvBootstrap.LoadRootEnvFile();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthenticatedUserAccessor, HttpAuthenticatedUserAccessor>();

builder.Services.AddControllers(
   options => options.Filters.Add<ExceptionFilter>()
);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string nije konfigurisan. Postavi ConnectionStrings__DefaultConnection ili SQL_* u .env.");
builder.Services.AddDbContext<SvadbeniSalonDbContext>(options =>
    options.UseSqlServer(connectionString)
);

builder.Services.AddMapster();

TypeAdapterConfig<User, UserResponse>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<UserUpdateRequest, User>.NewConfig().IgnoreNullValues(true);
TypeAdapterConfig<Recenzija, RecenzijaResponse>.NewConfig()
    .Map(dest => dest.PonudaNaziv, src => src.Ponuda != null ? src.Ponuda.Naziv : string.Empty)
    .Map(dest => dest.KorisnikIme, src => src.User != null
        ? $"{src.User.FirstName} {src.User.LastName}".Trim()
        : string.Empty)
    .Map(dest => dest.SvadbaDatum, src => src.Svadba != null ? src.Svadba.DatumSvadbe : (DateTime?)null);

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAccessManager, AccessManager>();
builder.Services.AddScoped<ICryptoService, CryptoService>();

builder.Services.AddScoped<IPonudaService, PonudaService>();
builder.Services.AddScoped<IMeniService, MeniService>();
builder.Services.AddScoped<IArtikalService, ArtikalService>();
builder.Services.AddScoped<IMuzicarService, MuzicarService>();
builder.Services.AddScoped<IDekoracijaService, DekoracijaService>();
builder.Services.AddScoped<IZanrService, ZanrService>();
builder.Services.AddScoped<IPreporukaService, PreporukaService>();
builder.Services.AddScoped<INotifikacijaService, NotifikacijaService>();
builder.Services.AddScoped<ISvadbaService, SvadbaService>();
builder.Services.AddScoped<IDnevniSastanakService, DnevniSastanakService>();
builder.Services.AddScoped<IRecenzijaService, RecenzijaService>();
builder.Services.AddScoped<IRataService, RataService>();
builder.Services.AddScoped<IRacunService, RacunService>();
builder.Services.AddScoped<IIzvjestajService, IzvjestajService>();

var rabbitHost = builder.Configuration["RabbitMQ:Host"]
    ?? throw new InvalidOperationException("RabbitMQ:Host nije konfigurisan (.env / RABBITMQ_HOST).");
var rabbitUser = builder.Configuration["RabbitMQ:Username"]
    ?? throw new InvalidOperationException("RabbitMQ:Username nije konfigurisan (.env).");
var rabbitPass = builder.Configuration["RabbitMQ:Password"]
    ?? throw new InvalidOperationException("RabbitMQ:Password nije konfigurisan (.env).");
var rabbitConnection = $"host={rabbitHost};username={rabbitUser};password={rabbitPass}";
builder.Services.AddSingleton<IBus>(_ =>
    RabbitHutch.CreateBus(rabbitConnection, x => x.EnableNewtonsoftJson()));
builder.Services.AddScoped<INotificationPublisher, RabbitMqNotificationPublisher>();

var jwtSecret = builder.Configuration["JwtToken:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JwtToken:SecretKey nije konfigurisan (.env / JWT_SECRET_KEY).");
}

builder.Services.AddScoped<IValidator<PonudaInsertRequest>, PonudaInsertValidator>();
builder.Services.AddScoped<IValidator<PonudaUpdateRequest>, PonudaUpdateValidator>();
builder.Services.AddScoped<IValidator<MeniInsertRequest>, MeniInsertValidator>();
builder.Services.AddScoped<IValidator<MeniUpdateRequest>, MeniUpdateValidator>();
builder.Services.AddScoped<IValidator<ArtikalInsertRequest>, ArtikalInsertValidator>();
builder.Services.AddScoped<IValidator<ArtikalUpdateRequest>, ArtikalUpdateValidator>();
builder.Services.AddScoped<IValidator<SvadbaInsertRequest>, SvadbaInsertValidator>();
builder.Services.AddScoped<IValidator<SvadbaUpdateRequest>, SvadbaUpdateValidator>();
builder.Services.AddScoped<IValidator<DnevniSastanakInsertRequest>, DnevniSastanakInsertValidator>();
builder.Services.AddScoped<IValidator<DnevniSastanakUpdateRequest>, DnevniSastanakUpdateValidator>();
builder.Services.AddScoped<IValidator<MuzicarInsertRequest>, MuzicarInsertValidator>();
builder.Services.AddScoped<IValidator<MuzicarUpdateRequest>, MuzicarUpdateValidator>();
builder.Services.AddScoped<IValidator<DekoracijaInsertRequest>, DekoracijaInsertValidator>();
builder.Services.AddScoped<IValidator<DekoracijaUpdateRequest>, DekoracijaUpdateValidator>();
builder.Services.AddScoped<IValidator<ZanrInsertRequest>, ZanrInsertValidator>();
builder.Services.AddScoped<IValidator<ZanrUpdateRequest>, ZanrUpdateValidator>();
builder.Services.AddScoped<IValidator<RecenzijaInsertRequest>, RecenzijaInsertValidator>();
builder.Services.AddScoped<IValidator<RecenzijaUpdateRequest>, RecenzijaUpdateValidator>();
builder.Services.AddScoped<IValidator<RataInsertRequest>, RataInsertValidator>();
builder.Services.AddScoped<IValidator<RacunInsertRequest>, RacunInsertValidator>();
builder.Services.AddScoped<IValidator<UserInsertRequest>, UserInsertValidator>();
builder.Services.AddScoped<IValidator<UserUpdateRequest>, UserUpdateValidator>();
builder.Services.AddScoped<IValidator<UserRegisterRequest>, UserRegisterValidator>();
builder.Services.AddScoped<IValidator<UserPasswordChangeRequest>, UserPasswordChangeValidator>();
builder.Services.AddScoped<IValidator<AdminSetPasswordRequest>, AdminSetPasswordValidator>();

builder.Services.AddOpenApi();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtToken:Issuer"],
        ValidAudience = builder.Configuration["JwtToken:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "Role",
    };
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IUserIdProvider, ClaimUserIdProvider>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        var configured = builder.Configuration["Cors:AllowedOrigins"]
            ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        var origins = (configured ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (origins.Length == 0)
        {
            origins =
            [
                "http://localhost:5121",
                "http://127.0.0.1:5121",
            ];
        }

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Svadbeni Salon Rimac API",
            Description = "API for Svadbeni Salon Rimac"
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { jwtSecurityScheme, Array.Empty<string>() }
        });
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SvadbeniSalonDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    const int maxAttempts = 15;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied.");
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}). Retrying...", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
