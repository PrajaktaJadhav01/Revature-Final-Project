using Consul;
using CustomerManagement;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Run this service on a fixed port so Consul can discover it reliably.
builder.WebHost.UseUrls("http://localhost:5292");

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Consul client (assumes Consul agent running at http://localhost:8500)
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(cfg =>
{
    cfg.Address = new Uri("http://localhost:8500");
}));

// Configure EF Core for the shared DbContext.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Health check endpoint used by Consul.
app.MapGet("/health", () => Results.Ok("Healthy"));

// Customer API endpoints

var consulClient = app.Services.GetRequiredService<IConsulClient>();
var registration = new AgentServiceRegistration()
{
    ID = $"customer-service-{Guid.NewGuid()}",
    Name = "customer-service",
    Address = "localhost",
    Port = 5292,
    Check = new AgentServiceCheck
    {
        HTTP = "http://localhost:5292/health",
        Interval = TimeSpan.FromSeconds(10),
        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
    }
};

try
{
    await consulClient.Agent.ServiceRegister(registration);

    app.Lifetime.ApplicationStopping.Register(() =>
    {
        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
    });
}
catch (Exception ex)
{
    Console.WriteLine($"WARNING: Could not register service with Consul: {ex.Message}");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/v1/customer", async (AppDbContext db) =>
{
    var customers = await db.Customers.Where(c => !c.IsDeleted).ToListAsync();
    return Results.Ok(customers);
});

app.MapGet("/api/v1/customer/{id:int}", async (int id, AppDbContext db) =>
{
    var customer = await db.Customers.FindAsync(id);
    return customer is not null ? Results.Ok(customer) : Results.NotFound();
});

app.MapPost("/api/v1/customer", async (Customer customer, AppDbContext db) =>
{
    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.Created($"/api/v1/customer/{customer.CustomerId}", customer);
});

app.MapPut("/api/v1/customer/{id:int}", async (int id, Customer updated, AppDbContext db) =>
{
    var existing = await db.Customers.FindAsync(id);
    if (existing is null)
        return Results.NotFound();

    existing.CustomerName = updated.CustomerName;
    existing.Email = updated.Email;
    existing.Phone = updated.Phone;
    existing.Website = updated.Website;
    existing.Industry = updated.Industry;
    existing.CompanySize = updated.CompanySize;
    existing.Type = updated.Type;
    existing.Classification = updated.Classification;
    existing.SegmentId = updated.SegmentId;
    existing.AccountValue = updated.AccountValue;
    existing.HealthScore = updated.HealthScore;
    existing.IsDeleted = updated.IsDeleted;
    existing.ModifiedDate = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(existing);
});

app.MapDelete("/api/v1/customer/{id:int}", async (int id, AppDbContext db) =>
{
    var existing = await db.Customers.FindAsync(id);
    if (existing is null)
        return Results.NotFound();

    existing.IsDeleted = true;
    existing.ModifiedDate = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
