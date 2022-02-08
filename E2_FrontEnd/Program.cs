using E2_FrontEnd.ActorDefine;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:5001");

// Add services to the container.
builder.Services.AddControllers().AddDapr();
builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<OrderStatusActor>();

    options.ActorIdleTimeout = TimeSpan.FromMinutes(60);
    options.ActorScanInterval = TimeSpan.FromSeconds(30);
    options.DrainOngoingCallTimeout = TimeSpan.FromSeconds(60);
    options.DrainRebalancedActors = true;
    options.RemindersStoragePartitions = 7;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});

app.UseAuthorization();
app.UseRouting();

app.MapControllers();
app.UseCloudEvents();
app.UseEndpoints((endpoints =>
{
    endpoints.MapSubscribeHandler();
    endpoints.MapActorsHandlers();
}));
app.Run();
