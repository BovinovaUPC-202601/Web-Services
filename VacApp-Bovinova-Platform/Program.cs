using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.RanchManagement.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.RanchManagement.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Interfaces.ASAP.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.StaffAdministration.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.CampaignManagement.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.CampaignManagement.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.CampaignManagement.Infrastructure.Repositories;
using VacApp_Bovinova_Platform.IAM.Application.OutBoundServices;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.IAM.Domain.Services;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Repositories;
using VacApp_Bovinova_Platform.IAM.Application.CommandServices;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Hashing.BCrypt.Services;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Tokens.JWT.Services;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Tokens.JWT.Configuration;
using Microsoft.OpenApi.Models;
using VacApp_Bovinova_Platform.IAM.Application.QueryServices;
using dotenv.net;
using VacApp_Bovinova_Platform.Shared.Application.OutboundServices;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Media.Cloudinary;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.Persistence.EFC.Repositories;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//configure Lower Case URLs
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Configure Kebab Case Route Naming Convention
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
    c =>
    {
        c.EnableAnnotations();

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    });

/////////////////////////Begin Database Configuration/////////////////////////

var connectionString = $"Server={Environment.GetEnvironmentVariable("DB_HOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                       $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                       $"User={Environment.GetEnvironmentVariable("DB_USER")};" +
                       $"Password={Environment.GetEnvironmentVariable("DB_PASS")};";

// Verify Database Connection string
if (string.IsNullOrEmpty(connectionString))
    throw new Exception("Database connection string is not set in .env");

// Configure Database Context and Logging Levels
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySQL(connectionString)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors()
    );
}
else if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySQL(connectionString)
               .LogTo(Console.WriteLine, LogLevel.Error)
               .EnableDetailedErrors()
    );
}

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configure Dependency Injection

// Shared Bounded Context Injection Configuration
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMediaStorageService, CloudinaryService>();

//IAM
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

//Ranch Management BC
builder.Services.AddScoped<IBovineRepository, BovineRepository>();
builder.Services.AddScoped<IBovineQueryService, BovineQueryService>();
builder.Services.AddScoped<IBovineCommandService, BovineCommandService>();

builder.Services.AddScoped<IStableRepository, StableRepository>();
builder.Services.AddScoped<IStableQueryService, StableQueryService>();
builder.Services.AddScoped<IStableCommandService, StableCommandService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryQueryService, CategoryQueryService>();
builder.Services.AddScoped<ICategoryCommandService, CategoryCommandService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductQueryService, ProductQueryService>();
builder.Services.AddScoped<IProductCommandService, ProductCommandService>();

//Staff Administration BC
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffQueryService, StaffQueryService>();
builder.Services.AddScoped<IStaffCommandService, StaffCommandService>();

//Campaign Management BC
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignCommandService, CampaignCommandService>();
builder.Services.AddScoped<ICampaignQueryService, CampaignQueryService>();

//IoT Monitoring BC
builder.Services.AddScoped<IBovineHealthRecordRepository, BovineHealthRecordRepository>();
builder.Services.AddScoped<IBovineHealthRecordCommandService, BovineHealthRecordCommandService>();
builder.Services.AddScoped<IBovineHealthRecordQueryService, BovineHealthRecordQueryService>();


/////////////////////////End Database Configuration/////////////////////////
var app = builder.Build();

// Verify Database Objects are created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllPolicy");
app.UseRequestAuthorization();
app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();
app.Run();