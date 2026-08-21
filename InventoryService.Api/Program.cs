using InventoryService.Api.Consumers;
using InventoryService.Api.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

// Run startup + Kestrel on a dedicated thread with a bigger stack (16 MB instead of the
// container default). This works around a startup crash (stack overflow / SIGSEGV) seen
// on some Docker/WSL2 setups while ASP.NET Core resolves the MassTransit/RabbitMQ
// hosted-service dependency graph. Safe to revert to a plain top-level Main once the
// underlying MassTransit/RabbitMQ issue is confirmed fixed upstream.
var startupThread = new Thread(Start, 16 * 1024 * 1024);
startupThread.Start();
startupThread.Join();
return;

void Start()
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddDbContext<InventoryDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHealthChecks();
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<OrderCreatedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
            });
            cfg.ConfigureEndpoints(context);
        });
    });
    var app = builder.Build();

    // Apply pending EF Core migrations automatically on startup (fine for a demo/dev setup;
    // use a dedicated migration step instead in a real production pipeline).
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        db.Database.Migrate();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
