using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Business.Services.Auth;
using Rogue_Kie.BE.Business.Services.Email;
using Rogue_Kie.BE.Business.Services.Roles;
using Rogue_Kie.BE.Business.Services.Users;
using Rogue_Kie.BE.Business.Services.Profile;
using Rogue_Kie.BE.Business.Services.RunHistoryService;
using Rogue_Kie.BE.DataAccess.DBContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Rogue_Kie.BE.Contracts.Jwt;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS - allow Unity editor / other origins during development
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "DevCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true) // Allow any origin
              .AllowAnyHeader();
    });
});

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IRunHistoryService, RunHistoryService>();

// Register GameConfig services
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.IEnemyService, Rogue_Kie.BE.Business.Services.GameConfigs.EnemyService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.IBulletService, Rogue_Kie.BE.Business.Services.GameConfigs.BulletService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.IWeaponService, Rogue_Kie.BE.Business.Services.GameConfigs.WeaponService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.ILevelService, Rogue_Kie.BE.Business.Services.GameConfigs.LevelService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.IBuffService, Rogue_Kie.BE.Business.Services.GameConfigs.BuffService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.IGameSyncService, Rogue_Kie.BE.Business.Services.GameConfigs.GameSyncService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.ICharacterService, Rogue_Kie.BE.Business.Services.GameConfigs.CharacterService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.ICosmeticService, Rogue_Kie.BE.Business.Services.GameConfigs.CosmeticService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.IShopItemService, Rogue_Kie.BE.Business.Services.GameConfigs.ShopItemService>();
builder.Services.AddScoped<Rogue_Kie.BE.Business.Services.GameConfigs.IPlayerWeaponService, Rogue_Kie.BE.Business.Services.GameConfigs.PlayerWeaponService>();

// Register PayOS Payment Service
builder.Services.Configure<Rogue_Kie.BE.Contracts.Payment.PayOSSettings>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddHttpClient<Rogue_Kie.BE.Business.Services.Payment.IPayOSService, Rogue_Kie.BE.Business.Services.Payment.PayOSService>();

// Add SignalR
builder.Services.AddSignalR();

// Configure JwtSettings from configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings != null && !string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rogue-Kie.BE API",
        Version = "v1",
        Description = "API documentation"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\nExample: \"Bearer eyJhb...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Tự động chạy Migration khi ứng dụng khởi động để đồng bộ DB trên Cloud / Azure
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("[Database] Tự động cập nhật Migrations database thành công.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Database Error] Lỗi tự động chạy migrations: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS policy for development before auth/authorization
app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<Rogue_Kie.BE.API.Hubs.GameHub>("/gamehub");

app.Run();
