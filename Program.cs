using CustomerApi.Common;
using CustomerApi.Data;
using CustomerApi.Services;
using CustomerApi.Validators;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NLog.Web;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper — scans for Profile classes (MappingProfile) and registers them.
builder.Services.AddAutoMapper(typeof(MappingProfile));


// FluentValidation — registers every validator in this assembly (both DTO validators)
// and hooks automatic validation into the MVC pipeline.
builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();


builder.Services.AddScoped<ICustomerService, CustomerService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
