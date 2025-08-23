using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using backend.Services;
using backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon.S3;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
var s3AccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
var s3SecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
var pgsqlConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // options.UseNpgsql(builder.Configuration.GetConnectionString("AwsRdsConnection"));
    options.UseNpgsql(pgsqlConnectionString);
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
});

builder.Services.AddControllers();

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    // 1. TIERED RATE LIMITING - Different limits for different endpoint types

    // Strict limits for authentication endpoints
    options.AddPolicy("AuthPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,           // Only 5 attempts
                Window = TimeSpan.FromMinutes(15) // Per 15 minutes
            }));

    // Moderate limits for regular API endpoints
    options.AddPolicy("ApiPolicy", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,         // 100 requests
                Window = TimeSpan.FromMinutes(1), // Per minute
                SegmentsPerWindow = 4      // Smoother than fixed window
            }));

    // Very lenient for read-only endpoints
    options.AddPolicy("ReadOnlyPolicy", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 200,          // Allow bursts
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 50,
                AutoReplenishment = true
            }));

    // 2. USER-AWARE RATE LIMITING - Higher limits for authenticated users
    options.AddPolicy("UserAwarePolicy", httpContext =>
    {
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
        var userId = httpContext.User.Identity?.Name;
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Use user ID if authenticated, IP if not
        var partitionKey = isAuthenticated ? $"user:{userId}" : $"ip:{ipAddress}";
        var permitLimit = isAuthenticated ? 500 : 50; // 10x more for authenticated users

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: partitionKey,
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4
            });
    });

    // 3. CONCURRENCY LIMITING for resource-heavy endpoints
    options.AddConcurrencyLimiter("ConcurrencyPolicy", options =>
    {
        options.PermitLimit = 10;          // Max 10 concurrent requests
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 20;           // Queue up to 20 more
    });

    // 4. NO RATE LIMITING for health checks, static files
    // (Use [DisableRateLimiting] attribute)
});

builder.Services.AddLogging();

var s3Client = new AmazonS3Client(new Amazon.Runtime.BasicAWSCredentials(s3AccessKey, s3SecretKey), Amazon.RegionEndpoint.APSoutheast1);

builder.Services.AddSingleton<IAmazonS3>(s3Client);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
    {
        Title = "Renta API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference()
                {
                    Id = "Bearer",
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
                }
            },
            []
            // new string[] {}
        }
    });
});

// Add JWT configuration
builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = jwtSecretKey!;
    options.Issuer = jwtIssuer!;
    options.Audience = jwtAudience!;
    options.ExpirationInMinutes = 60; // Token valid for 60 minutes
});


// Add JWT authentication
// var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecretKey!))
    };
});

// builder.Services.AddScoped<UserTaskService>();
builder.Services.AddScoped<UserService>();
// builder.Services.AddScoped<IEmailSender<User>, EmailSender>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<SubscriptionTierService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<SubscriptionTierService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<VehicleImageService>();
builder.Services.AddScoped<MediumTypeService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRentaWebFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowRentaWebFrontend");


app.Run();
