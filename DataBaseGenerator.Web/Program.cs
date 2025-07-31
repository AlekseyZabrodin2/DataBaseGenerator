using DataBaseGenerator.Core;
using DataBaseGenerator.Core.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BaseGenerateContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 28))
    ));

builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IWorklistService, WorklistService>();

var app = builder.Build();

app.Urls.Add("http://localhost:5289");
app.Urls.Add("https://localhost:7168");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
