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
        new MySqlServerVersion(new Version(8, 0, 21))));

// Repository pattern registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlayerStateRepository, PlayerStateRepository>();
builder.Services.AddScoped<IFarmStateRepository, FarmStateRepository>();
builder.Services.AddScoped<IRegularShopSlotRepository, RegularShopSlotRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPlayerRegularShopSlotRepository, PlayerRegularShopSlotRepository>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// Business logic services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IRegularShopSlotService, RegularShopSlotService>();
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

// Ensure database is created and seed admin account
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KunFarmDbContext>();
    dbContext.Database.EnsureCreated();
    
    // Seed admin account from configuration
    var seeder = scope.ServiceProvider.GetRequiredService<KunFarm.BLL.Services.DatabaseSeederService>();
    await seeder.SeedAsync();
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