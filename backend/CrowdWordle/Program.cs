using CrowdWordle;
using CrowdWordle.BackgroundServices;
using CrowdWordle.Data;
using CrowdWordle.Services;
using CrowdWordle.Shared;

var builder = WebApplication.CreateSlimBuilder(args);
var dbconn = builder.Configuration["WordleDbConnection"]!;
var connectionstring = $"Data Source={dbconn};Cache=Shared;";
Console.WriteLine($"Connection String used: {connectionstring}");
builder.Services.AddScoped(_ =>
    new DbService(connectionstring));

builder.Services.AddSingleton<SystemStatus>(sp =>
{
    using var scope = sp.CreateScope();
    using var db = scope.ServiceProvider.GetRequiredService<DbService>();

    var systemRecord = db.QuerySingle(
        "SELECT UserIdIndex, HighestUserCount  FROM SystemRecords",
        reader => new SystemRecord(
            (ulong)reader.GetInt64(0),
            (ulong)reader.GetInt64(1)
        )
    );
    return new SystemStatus
    {
        UserIdIndex = systemRecord?.UserIdIndex ?? 0UL,
        HighestUserCount = systemRecord?.HighestUserCount ?? 0UL,
    };
});

builder.Services.Configure<GameConfiguration>(builder.Configuration.GetSection("Game"));
builder.Services.Configure<TokenConfiguration>(builder.Configuration.GetSection("Token"));

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<GameEngine>();
builder.Services.AddSingleton<WordService>();
builder.Services.AddSingleton<VotingService>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddHostedService<GameLoopService>();
builder.Services.AddHostedService<ConnectionCleanupService>();
builder.Services.AddSingleton(provider =>
        new VoteStreamingService(
            maxWordsPerSecond: 5,
            maxQueueSize: 1000
        ));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors();
}

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

var api = app.MapGroup("/api");

api.MapPost("/auth", async (VerifyRequest request, TokenService tokenService, DbService db, CancellationToken ct) =>
{
    if (!string.IsNullOrWhiteSpace(request.Token) && tokenService.ValidateToken(request.Token))
    {
        return Results.Text(request.Token, "text/plain");
    }

    var (token, userId) = tokenService.GenerateToken();

    await db.ExecuteNonQueryAsync("UPDATE SystemRecords SET UserIdIndex = @userid", ("@userid", userId));

    return Results.Text(token, "text/plain");
});

api.MapGet("/tagline", (WordService service) =>
{
    return Results.Text(service.GetTagLine(), "text/plain");
});

api.MapGet("/status", (SystemStatus system, ConnectionManager connectionManager, GameEngine gameEngine) =>
{
    var game = gameEngine.GetCurrentGame();
    var status = new Status(connectionManager.GetConnectionCount(), game.State.ToString(), game.Round);
    return Results.Ok(status);
});

api.MapGet("/health", () => Results.Ok("Healthy"));

api.Map("/ws", async (HttpContext context, ConnectionManager connectionManager, TokenService tokenService, GameEngine gameEngine) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var token = context.Request.Query["token"].ToString();
    if (string.IsNullOrEmpty(token))
    {
        context.Response.StatusCode = 401;
        return;
    }

    var userId = tokenService.ValidateAndGetUserId(token);
    if (userId == 0)
    {
        context.Response.StatusCode = 401;
        return;
    }

    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    await connectionManager.HandleConnectionAsync(userId, webSocket, gameEngine);
});

app.Run();