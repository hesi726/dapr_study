var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:5001");

// Add services to the container.
builder.Services.AddControllers().AddDapr();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
