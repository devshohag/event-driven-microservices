using MassTransit;
using NotificationService.Api.Consumers;

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
    builder.Services.AddOpenApi();
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

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
