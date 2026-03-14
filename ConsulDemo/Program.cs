using Consul;

var builder = WebApplication.CreateBuilder(args);

// Run the Consul demo service on a fixed port so we can register it reliably.
builder.WebHost.UseUrls("http://localhost:7000");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(cfg =>
{
    cfg.Address = new Uri("http://localhost:8500");
}));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

// Simple health-check endpoint used by Consul to verify service health.
app.MapGet("/health", () => Results.Ok("Healthy"));

var consulClient = app.Services.GetRequiredService<IConsulClient>();

var registration = new AgentServiceRegistration()
{
    ID = $"order-service-{Guid.NewGuid()}",
    Name = "order-service",
    Address = "localhost",
    Port = 7000,
    Check = new AgentServiceCheck
    {
        HTTP = "http://localhost:7000/health",
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

app.Run();