using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Mapping;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// 1. DEPENDENCY INJECTION — Register all services with the DI container.
//    AddScoped = one instance per HTTP request (correct for EF Core contexts).
// -----------------------------------------------------------------------

// EF Core: Connect to SQL Server LocalDB using the connection string from appsettings.json
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository: IProductRepository → ProductRepository
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Service: IProductService → ProductService
builder.Services.AddScoped<IProductService, ProductService>();

// AutoMapper: Scans the assembly for all Profile classes (finds MappingProfile automatically)
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Controllers with JSON serialization
builder.Services.AddControllers();

// Global Exception Handling (using .NET 8 IExceptionHandler)
builder.Services.AddExceptionHandler<ProductCatalog.Api.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger / OpenAPI for interactive API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Product Catalog API", Version = "v1" });
});

// -----------------------------------------------------------------------
// 2. CORS — Allow requests from the Angular dev server (localhost:4200).
//    In production, replace with your actual frontend domain.
// -----------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// -----------------------------------------------------------------------
// 3. MIDDLEWARE PIPELINE — Order matters here!
// -----------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Catalog API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at the root URL
    });
}

// -----------------------------------------------------------------------
// GLOBAL EXCEPTION HANDLER
// Must be early in the pipeline to catch errors from downstream middleware
// -----------------------------------------------------------------------
app.UseExceptionHandler();

app.UseHttpsRedirection();

// CORS must come BEFORE UseAuthorization and MapControllers
app.UseCors("AllowAngularDev");

app.UseAuthorization();

app.MapControllers();

// -----------------------------------------------------------------------
// 4. AUTO MIGRATE — Apply pending migrations on startup (dev convenience).
//    In production, use a dedicated migration strategy instead.
// -----------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();
}

app.Run();
