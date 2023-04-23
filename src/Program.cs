using Microsoft.EntityFrameworkCore;
using src.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PriceDetailDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PriceDetailDbContext") ?? throw new InvalidOperationException("Connection string 'PriceDetailDbContext' not found.")));

/*If db is empty. Load from CSV.*//*
var context = builder.Services.BuildServiceProvider().GetRequiredService<PriceDetailDbContext>();
if (!context.PriceInfo.Any())
{
    var data = new LoadData(context);
    data.LoadDataFromCsv();
}
Console.WriteLine("DataBase populated with " + context.PriceInfo.Count() + " rows.");*/
// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();