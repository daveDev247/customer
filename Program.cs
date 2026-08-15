using CustomerApi.Common;
using CustomerApi.Data;
using CustomerApi.Services;
using CustomerApi.Services.Interface;
using CustomerApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using System.Text;



var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Logging.ClearProviders();

// Set the log directory via Global Diagnostics Context BEFORE UseNLog() runs.
// GDC is independent of NLog's Configuration object, so it survives the
// re-initialization that happens later during host startup — unlike
// Configuration.Variables, which gets reset when the config reloads.
NLog.GlobalDiagnosticsContext.Set("logDirectory", builder.Configuration["Logging:LogPath"] ?? "logs");
builder.Host.UseNLog();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// AutoMapper — scans for Profile classes (MappingProfile) and registers them.
builder.Services.AddAutoMapper(typeof(MappingProfile));


// FluentValidation — registers every validator in this assembly (both DTO validators)
// and hooks automatic validation into the MVC pipeline.
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerDtoValidator>();
//builder.Services.AddFluentValidationAutoValidation();


builder.Services.AddScoped<ICustomerService, CustomerService>();
// Register the new auth services alongside the existing CustomerService registration.
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Bind the JwtSettings section so IOptions<JwtSettings> resolves everywhere it's injected.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Defines the "Bearer" auth scheme Swagger UI will show as a padlock/Authorize button.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter ONLY the raw JWT token (no 'Bearer ' prefix — Swagger adds that automatically)."
    });

    // Tells Swagger to actually attach the token as an Authorization header
    // on every request made from the UI, once you've clicked Authorize.
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
            Array.Empty<string>()
        }
    });
});

// Configures the app to validate incoming JWTs on any endpoint marked [Authorize] —
// none are marked yet (existing Customer endpoints untouched), but this makes the
// mechanism available for the next stage.
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler sits early in the pipeline so it can catch anything
// thrown further down — controllers, services, EF Core, everything.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();


// UseAuthentication MUST come before UseAuthorization — authentication figures out
// "who is this," authorization figures out "are they allowed to do this."
// Getting the order wrong means [Authorize] checks run against an unauthenticated context.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
