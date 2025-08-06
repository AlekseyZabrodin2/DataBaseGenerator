using System.Net.Http.Headers;
using DataBaseGenerator.Core.Data;
using DataBaseGenerator.Web.Controllers.ApiControllers;
using DataBaseGenerator.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5289");
//builder.WebHost.UseUrls("http://localhost:5289", "https://localhost:7168");

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
builder.Services.AddScoped<PatientApiController>();

builder.Services.AddHttpClient("DBGeneratorApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5289/api/");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

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
