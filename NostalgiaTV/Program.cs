using NostalgiaTV.Hubs;
using NostalgiaTV.Models.Configuration;
using NostalgiaTV.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR(e =>
{
    e.EnableDetailedErrors = true;
    e.MaximumReceiveMessageSize = long.MaxValue;
});

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

//configurations
builder.Services.Configure<List<ChannelConfiguration>>(builder.Configuration.GetSection("Channels"));
builder.Services.Configure<AppConfiguration>(builder.Configuration.GetSection("App"));

//services
builder.Services.AddHostedService<ChannelService>();
builder.Services.AddHostedService<EpisodeService>();
builder.Services.AddSingleton<ChannelsHub>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
app.UseCors(builder => builder
   .WithOrigins(corsOrigins)
   .AllowAnyMethod()
   .AllowAnyHeader()
   .AllowCredentials());

app.MapHub<ChannelsHub>("/channels");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);



app.Run();
