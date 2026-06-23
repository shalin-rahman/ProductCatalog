# Full-Stack Guide: .NET 8 & Angular 19 — Condensed Reference

---

## 0. Solution & Project File Structure

### Files Overview

| File | Purpose |
|---|---|
| `ProductCatalog.sln` | Workspace index — lists all projects, no C# code |
| `ProductCatalog.Api.csproj` | Build blueprint — target framework, NuGet packages |
| `ProductCatalog.Api.Tests.csproj` | Test project — references the API project directly |

### API `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>         <!-- string ≠ string? -->
    <ImplicitUsings>enable</ImplicitUsings>  <!-- auto-injects System, Linq, Tasks, etc. -->
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="12.0.1" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>   <!-- dev-time only, not shipped -->
    </PackageReference>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  </ItemGroup>
</Project>
```

### Dependency Graph

```
ProductCatalog.sln
    ├── ProductCatalog.Api          (Sdk.Web, net8.0)
    │       ├── AutoMapper, EF Core, Swashbuckle
    └── ProductCatalog.Api.Tests    (Sdk, net9.0)
            ├── NUnit, Moq, coverlet.collector
            └── ──→ ProjectReference → ProductCatalog.Api
```

> Tests target `net9.0` for latest NUnit; API targets `net8.0` LTS. .NET is backwards-compatible.

### .NET Runtime Pipeline

1. **Roslyn** compiles C# → IL (platform-neutral bytecode)
2. **CoreCLR** loads the assembly, resolves deps, starts the GC
3. **JIT** compiles IL → native machine code on first call (why first request is slower)
4. **Execution** — GC manages memory in Gen0/1/2 generations; ThreadPool handles async work

---

## 1. Technology Choices

### .NET 8 Backend

- LTS release; one of the fastest web frameworks on TechEmpower benchmarks
- First-class DI, minimal APIs, EF Core ORM, NativeAOT support
- `ImplicitUsings` auto-injects `System`, `Linq`, `Tasks`, etc. into every file
- `Nullable enable` — `string` cannot be null; `string?` explicitly allows null

### Angular 19 Frontend

- Standalone components — no `NgModule` bottleneck
- Built-in `@for`/`@if` control flow (up to 90% faster than `*ngFor`/`*ngIf`)
- `provideHttpClient(withFetch())` — native Fetch API instead of XHR
- Reactive Forms with TypeScript-side validation
- Lazy-loaded routes via `loadComponent`

---

## 2. Architecture

### Request Flow

```
Browser → Angular UI → HttpClient → .NET Kestrel → Middleware Pipeline
    → ProductsController → ProductService → ProductRepository → EF Core → SQL Server
```

In production: NGINX sits in front of both, serving static Angular files and proxying `/api/*` to Kestrel.

### Clean Architecture Layers

```
Controller   → IProductService
Service      → IProductRepository
Repository   → CatalogDbContext
DbContext    → SQL Server
```

Each layer only knows about the layer directly below it via an **interface**. Swapping SQL Server for PostgreSQL only touches `ProductRepository.cs`.

### DTOs vs Entities

| Type | File | Direction | Purpose |
|---|---|---|---|
| `Product` | `Domain/Product.cs` | Internal | Database row shape |
| `ProductDto` | `DTOs/ProductDtos.cs` | Outbound (GET) | Read model, includes `Id` |
| `ProductCreateDto` | `DTOs/ProductDtos.cs` | Inbound (POST) | No `Id` — SQL Server assigns it |
| `ProductUpdateDto` | `DTOs/ProductDtos.cs` | Inbound (PUT) | No `Id` — comes from URL route |

DTOs prevent over-posting: a user cannot send `{"isApproved": true}` if the field isn't in the DTO.

---

## 3. Directory Structure

### Backend

```
ProductCatalog.Api/
├── Controllers/ProductsController.cs
├── Data/CatalogDbContext.cs + Migrations/
├── Domain/Product.cs
├── DTOs/ProductDtos.cs
├── Mapping/MappingProfile.cs
├── Middleware/GlobalExceptionHandler.cs
├── Repositories/IProductRepository.cs + ProductRepository.cs
├── Services/IProductService.cs + ProductService.cs
└── Program.cs
```

### Frontend

```
product-catalog-ui/src/app/
├── app.component.ts          # Root shell (navbar + router-outlet)
├── app.config.ts             # provideHttpClient, provideRouter
├── app.routes.ts             # Lazy-loaded route definitions
├── core/services/product.service.ts
├── features/products/
│   ├── product-list/         # Route: /products
│   └── product-form/         # Routes: /products/new AND /products/edit/:id
└── shared/models/product.model.ts
environments/environment.ts + environment.development.ts
```

---

## 3.5 Dependency Injection in Depth

### Registration (Program.cs)

```csharp
builder.Services.AddDbContext<CatalogDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
```

### Service Lifetimes

| Lifetime | Method | Lives for | Use for |
|---|---|---|---|
| Transient | `AddTransient` | Every injection | Stateless utilities |
| **Scoped** | `AddScoped` | One HTTP request | DbContext, repos, services |
| Singleton | `AddSingleton` | App lifetime | Thread-safe caches, config |

Everything here is **Scoped** because `DbContext` is Scoped. A singleton cannot hold a scoped dependency — the container throws at runtime.

### Constructor Injection

```csharp
// Controller never calls `new ProductService()`
public ProductsController(IProductService service) { _service = service; }

// In tests, inject a mock — controller code is identical
var mockService = new Mock<IProductService>();
var controller = new ProductsController(mockService.Object);
```

---

## 4. EF Core: Code-First Migrations

```
1. Write C# model: Product { Id, Name, Price, Description }
2. Register in DbContext: DbSet<Product> Products
3. dotnet ef migrations add InitialCreate   → generates CREATE TABLE script
4. dotnet ef database update               → runs it against SQL Server
5. Add property → dotnet ef migrations add AddStockToProduct → ALTER TABLE (delta only)
```

**`__EFMigrationsHistory`** — EF Core records each applied migration here. `db.Database.Migrate()` on startup only runs unapplied ones, making it safe to call every boot.

**Change Tracking:**
```csharp
var product = await _context.Products.FindAsync(1);  // EF tracks this
product.Price = 99.99m;
await _context.SaveChangesAsync();
// SQL: UPDATE Products SET Price=99.99 WHERE Id=1  (only changed columns)
```

**`AsNoTracking()` on reads** — skips the ChangeTracker, 20–30% faster, less memory:
```csharp
return await _context.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
```

---

## 5. AutoMapper

Three mappings in `MappingProfile.cs`:

| Mapping | Used when | Notes |
|---|---|---|
| `Product → ProductDto` | GET responses | Includes `Id` |
| `ProductCreateDto → Product` | POST | No `Id`; SQL Server generates it |
| `ProductUpdateDto → Product` | PUT | `Id` set manually from URL: `product.Id = id` |

AutoMapper maps by **matching property names** automatically — no per-property config needed when names are identical.

---

## 6. Middleware Pipeline

```
Incoming Request
    ├── app.UseExceptionHandler()      ← MUST be first (wraps all below)
    ├── app.UseHttpsRedirection()
    ├── app.UseCors("AllowAngularDev") ← Must come before UseAuthorization
    ├── app.UseAuthorization()
    └── app.MapControllers()
Response flows back UP through the same chain
```

**Order is critical:** `UseCors` before `UseAuthorization` — otherwise browser preflight `OPTIONS` requests (no auth token) are rejected before CORS headers are added, and you see a CORS error instead of a 401.

---

## 7. API Endpoint Reference

| Method | Endpoint | Body | Success | Errors |
|---|---|---|---|---|
| `GET` | `/api/products` | — | `200` + `ProductDto[]` | — |
| `GET` | `/api/products/{id}` | — | `200` + `ProductDto` | `404` |
| `POST` | `/api/products` | `ProductCreateDto` | `201` + `ProductDto` | `400` |
| `PUT` | `/api/products/{id}` | `ProductUpdateDto` | `204` | `400`, `404` |
| `DELETE` | `/api/products/{id}` | — | `204` | `404` |

`[ApiController]` automatically validates DTOs against Data Annotations and returns `400 ValidationProblemDetails` if invalid — no manual `ModelState.IsValid` needed.

| Helper | HTTP | Use |
|---|---|---|
| `Ok(data)` | 200 | GET success |
| `CreatedAtAction(...)` | 201 | POST — sets `Location` header to new resource URL |
| `NoContent()` | 204 | PUT/DELETE success |
| `NotFound(obj)` | 404 | Resource missing |
| `BadRequest(obj)` | 400 | Invalid input |

---

## 8. Quick Start

### Prerequisites
- .NET 8 SDK, Node.js v24+, SQL Server LocalDB, Git

### Backend
```bash
cd ProductCatalog.Api
dotnet restore
dotnet ef database update   # or let Program.cs auto-migrate on startup
dotnet run
# Swagger at: https://localhost:7001  (RoutePrefix = string.Empty → root)
```

### Frontend
```bash
cd product-catalog-ui
npm install
npm start
# App at: http://localhost:4200
```

---

## 9. Common Issues

### CORS Error
Angular on `localhost:4200` is blocked from fetching `localhost:7001` by the browser's Same-Origin Policy. Fix: `UseCors("AllowAngularDev")` in middleware and policy configured in `Program.cs`.

### EF Core Migration Fails
Check `appsettings.json` connection string. LocalDB instance name must match (`(localdb)\\MSSQLLocalDB`).

### Angular Form Not Submitting
Check `form.valid` — if validation fails and the user never touched a field, call `form.markAllAsTouched()` to surface all errors.

### 401 on Preflight OPTIONS
`UseCors` is registered after `UseAuthorization`. Swap order.

---

## 10. Full Code Reference

### `Program.cs`
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CatalogDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(opts => opts.AddPolicy("AllowAngularDev",
    p => p.WithOrigins("http://localhost:4200")
          .AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();   // auto-migrate on every startup
}

app.Run();
```

### `Domain/Product.cs`
```csharp
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
}
```

### `DTOs/ProductDtos.cs`
```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

public class ProductCreateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}

public class ProductUpdateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}
```

### `Mapping/MappingProfile.cs`
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();
    }
}
```

### `Middleware/GlobalExceptionHandler.cs`
```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Unhandled exception");   // raw details stay in server logs
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 500,
            Title = "An unexpected error occurred.",
            Detail = "Please try again later."          // never expose ex.Message to clients
        }, ct);
        return true;
    }
}
```

### `app/app.config.ts`
```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withFetch()),   // native Fetch API instead of XHR
  ]
};
```

### `app/app.routes.ts`
```typescript
export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products',
    loadComponent: () => import('./features/products/product-list/product-list.component')
      .then(m => m.ProductListComponent) },
  { path: 'products/new',
    loadComponent: () => import('./features/products/product-form/product-form.component')
      .then(m => m.ProductFormComponent) },
  { path: 'products/edit/:id',
    loadComponent: () => import('./features/products/product-form/product-form.component')
      .then(m => m.ProductFormComponent) },
  { path: '**', redirectTo: 'products' }
];
```

### `core/services/product.service.ts`
```typescript
@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = `${environment.apiUrl}/products`;
  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]>          { return this.http.get<Product[]>(this.apiUrl); }
  getById(id: number): Observable<Product> { return this.http.get<Product>(`${this.apiUrl}/${id}`); }
  create(p: ProductCreate): Observable<Product> { return this.http.post<Product>(this.apiUrl, p); }
  update(id: number, p: ProductUpdate): Observable<void>
    { return this.http.put<void>(`${this.apiUrl}/${id}`, p); }
  delete(id: number): Observable<void>
    { return this.http.delete<void>(`${this.apiUrl}/${id}`); }
}
```

### `shared/models/product.model.ts`
```typescript
export interface Product       { id: number; name: string; description?: string; price: number; }
export interface ProductCreate { name: string; description?: string; price: number; }
export interface ProductUpdate { name: string; description?: string; price: number; }
```

### `environments/`
```typescript
// environment.ts (production)
export const environment = { apiUrl: '/api' };

// environment.development.ts (ng serve)
export const environment = { apiUrl: 'https://localhost:7001/api' };
```

---

## 11. C# Language Features

### Interfaces
Interfaces are contracts with no code. The controller depends on `IProductService` (the contract); the DI container resolves it to `ProductService` (the implementation). In tests, Moq provides `Mock<IProductService>().Object` — same interface, no real database.

### async/await State Machine
`await` is compiled into a state machine that calls `MoveNext()`. The thread is **released back to the ThreadPool** when awaiting I/O. When the database responds, any available thread resumes. Zero blocking.

### Pattern Matching & Null Safety
```csharp
if (product is null) return NotFound(...);        // pattern matching
string? name = product?.Name;                     // null-conditional
string display = product?.Name ?? "Unknown";      // null-coalescing
```

### Data Annotations
```csharp
[Required(ErrorMessage = "Name is required.")]
[MaxLength(100)]
public string Name { get; set; } = string.Empty;

[Range(0.01, double.MaxValue)]
public decimal Price { get; set; }
```
`[ApiController]` runs these before the action method. Invalid requests are rejected with `400 ValidationProblemDetails` automatically.

### `decimal` vs `double`
- `double`: binary float — `0.1 + 0.2 = 0.30000000000000004`. Never use for money.
- `decimal`: base-10 float — `0.1m + 0.2m == 0.3m`. Always use for currency.

---

## 12. Angular Concepts

### Reactive Forms
```typescript
this.productForm = this.fb.group({
  name:  ['', [Validators.required, Validators.maxLength(100)]],
  price: [null, [Validators.required, Validators.min(0.01)]]
});
```
Form state lives in TypeScript — testable without DOM rendering. `markAllAsTouched()` on submit forces all error messages visible.

| | Template-Driven | Reactive |
|---|---|---|
| Validation defined in | HTML | TypeScript |
| Testability | Needs DOM | Pure TypeScript |
| Complex/conditional rules | Messy | Scales well |

### Lazy Loading
`loadComponent` splits each route into a separate JS chunk. Browser downloads `main.js` on load, then each component chunk **only on first navigation**. 50-route app: initial payload is 50× smaller.

### `@for` / `@if` (Angular 17+)
```html
@for (product of products; track product.id) {
  <tr><td>{{ product.name }}</td></tr>
}
@if (isLoading) { <div class="spinner-border"></div> }
@else { <table>...</table> }
```
`track product.id` — Angular only re-renders changed rows, not the entire list.

### Change Detection
Zone.js patches all async APIs to notify Angular after every event/HTTP call. **Signals** (Angular 16+) are the next-gen alternative: fine-grained tracking — only the exact component that reads a signal is re-checked.

### Component Lifecycle

| Hook | When | Use for |
|---|---|---|
| `constructor` | Instantiated | Inject services only |
| `ngOnInit` | After `@Input()` set | Load initial data |
| `ngOnDestroy` | Before removal | Unsubscribe long-lived Observables |

---

## 13. Tests

### `ProductServiceTests.cs`
```csharp
[TestFixture]
public class ProductServiceTests
{
    private Mock<IProductRepository> _mockRepo;
    private IMapper _mapper;
    private ProductService _service;

    [SetUp]
    public void SetUp()
    {
        _mockRepo = new Mock<IProductRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _service = new ProductService(_mockRepo.Object, _mapper);
    }

    [Test]
    public async Task GetAllProductsAsync_ReturnsAllProducts_MappedToDto()
    {
        var products = new List<Product> { new() { Id=1, Name="A", Price=10m } };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = await _service.GetAllProductsAsync();

        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("A"));
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Test]
    public async Task GetProductByIdAsync_WhenNotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);
        var result = await _service.GetProductByIdAsync(99);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateProductAsync_MapsDto_CreatesAndReturnsProduct()
    {
        var dto = new ProductCreateDto { Name = "New", Price = 5m };
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                 .ReturnsAsync((Product p) => { p.Id = 42; return p; });

        var result = await _service.CreateProductAsync(dto);

        Assert.That(result.Id, Is.EqualTo(42));
        Assert.That(result.Name, Is.EqualTo("New"));
    }
}
```

Run: `dotnet test --collect:"XPlat Code Coverage"`

---

## 14. End-to-End Request Trace: Edit Product

```
Click "Update Product"
  → Angular: form.valid? YES → isEditMode? YES
  → productService.update(3, formValue)
  → HTTP PUT https://localhost:7001/api/products/3  { name, description, price }

Kestrel receives request
  → ExceptionHandler wraps everything
  → CORS adds Access-Control-Allow-Origin header
  → MapControllers routes to ProductsController.Update(id:3, dto)
  → [ApiController] validates dto — all OK

ProductsController.Update
  → await _service.UpdateProductAsync(3, dto)

ProductService.UpdateProductAsync
  → product = _mapper.Map<Product>(dto)
  → product.Id = 3
  → return await _repository.UpdateAsync(product)

ProductRepository.UpdateAsync
  → AnyAsync: SELECT … WHERE Id=3 → true
  → _context.Products.Update(product) — marks Modified
  → SaveChangesAsync: UPDATE Products SET Name=@0, Description=@1, Price=@2 WHERE Id=3
  → return true

Response: 204 No Content
  → Angular subscribe next: this.router.navigate(['/products'])
  → ProductListComponent.ngOnInit → GET /api/products → re-renders table
```

**Error path:** `UpdateAsync` returns `false` → controller returns `NotFound` → Angular `error` callback sets `errorMessage` → `@if (errorMessage)` shows it.
