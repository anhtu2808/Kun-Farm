using KunFarm.DAL.Data;
using KunFarm.DAL.Interfaces;
using KunFarm.DAL.Repositories;
using KunFarm.BLL.Interfaces;
using KunFarm.BLL.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Database configuration - MySQL
builder.Services.AddDbContext<KunFarmDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("Default"), 
        new MySqlServerVersion(new Version(8, 0, 21)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// Repository pattern registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlayerStateRepository, PlayerStateRepository>();
builder.Services.AddScoped<IPlayerToolbarRepository, PlayerToolbarRepository>();
builder.Services.AddScoped<IFarmStateRepository, FarmStateRepository>();
builder.Services.AddScoped<IRegularShopSlotRepository, RegularShopSlotRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPlayerRegularShopSlotRepository, PlayerRegularShopSlotRepository>();
builder.Services.AddScoped<IInventorySlotRepository, InventorySlotRepository>();
builder.Services.AddScoped<IOnlineShopRepository, OnlineShopRepository>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// Business logic services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IRegularShopSlotService, RegularShopSlotService>();
builder.Services.AddScoped<IInventorySlotService, InventorySlotService>();
builder.Services.AddScoped<IOnlineShopService, OnlineShopService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<KunFarm.BLL.Services.DatabaseSeederService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? "sD1IIRal7ci9wDkdJSUKilPtWvi7LDDC5GlgjXNArl8=";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "KunFarm API",
        Version = "v1",
        Description = "API cho Unity 2D Farm Game",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "KunFarm Team",
            Email = "admin@kunfarm.com"
        }
    });

    // JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KunFarm API v1");
        c.RoutePrefix = "swagger"; // Swagger UI sẽ có sẵn tại /swagger
        c.DocumentTitle = "KunFarm API Documentation";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Ensure database is created and seed admin account with retry logic
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KunFarmDbContext>();
    
    // Retry logic for database connection
    int retryCount = 0;
    const int maxRetries = 10;
    const int delaySeconds = 5;
    
    while (retryCount < maxRetries)
    {
        try
        {
            Console.WriteLine($"Attempting to connect to database... (Attempt {retryCount + 1}/{maxRetries})");
            dbContext.Database.EnsureCreated();
            Console.WriteLine("✅ Database connected and created successfully!");
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            Console.WriteLine($"❌ Database connection failed (Attempt {retryCount}/{maxRetries}): {ex.Message}");
            
            if (retryCount >= maxRetries)
            {
                Console.WriteLine("💥 Max retries reached. Unable to connect to database.");
                throw;
            }
            
            Console.WriteLine($"⏳ Waiting {delaySeconds} seconds before retry...");
            await Task.Delay(delaySeconds * 1000);
        }
    }
    
    // Seed admin account from configuration
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<KunFarm.BLL.Services.DatabaseSeederService>();
        await seeder.SeedAsync();
        Console.WriteLine("✅ Database seeding completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Database seeding failed: {ex.Message}");
        // Don't throw here, app can still run without seeding
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();