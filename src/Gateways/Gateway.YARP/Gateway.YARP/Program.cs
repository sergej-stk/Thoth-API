var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddRateLimiter(rateLimiterOptions => rateLimiterOptions
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
        
        Log.Debug("RateLimiter configured with PermitLimit: {PermitLimit}, Window: {Window}, QueueLimit: {QueueLimit}", 
            options.PermitLimit, options.Window, options.QueueLimit);
    }));

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseHttpsRedirection();

app.MapReverseProxy();

app.Run();