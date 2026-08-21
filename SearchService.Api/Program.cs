using SearchService.Api.GraphQL;

// Run startup + Kestrel on a dedicated thread with a bigger stack (16 MB instead of the
// container default). HotChocolate does a lot of deep reflection while building the
// GraphQL schema at startup, which was overflowing the default stack in this environment
// (same root cause as the earlier Order/Inventory/Notification crash).
var startupThread = new Thread(Start, 16 * 1024 * 1024);
startupThread.Start();
startupThread.Join();
return;

void Start()
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks();

    builder.Services
        .AddGraphQLServer()
        .AddQueryType<Query>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapGraphQL();
    app.MapHealthChecks("/health");

    app.Run();
}
