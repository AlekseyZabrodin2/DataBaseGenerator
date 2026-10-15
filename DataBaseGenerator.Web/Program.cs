using System.Diagnostics;
using System.Net.Http.Headers;
using DataBaseGenerator.Core.MySqlGenerator;
using DataBaseGenerator.Core.MySqlGenerator.Data;
using DataBaseGenerator.Web.Controllers.ApiControllers;
using DataBaseGenerator.Web.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;



var logger = LogManager.Setup().LoadConfigurationFromFile("Nlog.config", optional: false).GetCurrentClassLogger();

var processName = Process.GetCurrentProcess().ProcessName;
var existingProcesses = Process.GetProcessesByName(processName)
                               .Where(p => p.Id != Process.GetCurrentProcess().Id);

if (existingProcesses.Any())
{
    logger.Info($"Found {existingProcesses.Count()} other instance(s) of {processName}, stopping them...");

    foreach (var process in existingProcesses)
    {
        try
        {
            process.Kill();
            process.WaitForExit();
            logger.Info($"Process {process.Id} stopped successfully.");
        }
        catch (Exception ex)
        {
            logger.Warn(ex, $"Failed to stop process {process.Id}");
        }
    }

    logger.Info("All previous web host instances stopped. Starting new instance...");
}

try
{
    logger.Trace("Initialization WebApplication");

    var builder = WebApplication.CreateBuilder(args);

    var configuration = AppConfiguration.LoadWebConfiguration();
    builder.Configuration.AddConfiguration(configuration);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var webHostUrl = builder.Configuration["WebHost:Uri"] ?? "http://localhost:5289";

    builder.WebHost.UseUrls(webHostUrl);
    //builder.WebHost.UseUrls("http://localhost:5289", "https://localhost:7168");

    builder.Services.AddControllersWithViews();

    var root = (IConfigurationRoot)builder.Configuration;

    foreach (var provider in root.Providers)
    {
        if (provider.TryGet("ConnectionStrings:DefaultConnection", out var value))
        {
            logger.Info($"PROVIDER: {provider}");
            logger.Info($"VALUE: {value}");
        }
    }

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    logger.Info($"Web.ConnectionString = {connectionString}");

    builder.Services.AddDbContext<BaseGenerateContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new MySqlServerVersion(new Version(8, 0, 28))
        ));

    builder.Services.AddScoped<IPatientService, DataBaseGenerator.Web.Services.PatientService>();
    builder.Services.AddScoped<IWorklistService, DataBaseGenerator.Web.Services.WorklistService>();
    builder.Services.AddScoped<PatientApiController>();

    builder.Services.AddHttpClient("DBGeneratorApi", client =>
    {
        var webHostApiUrl = builder.Configuration["WebHost:ApiUri"] ?? "http://localhost:5289/api/";

        client.BaseAddress = new Uri(webHostApiUrl);
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

    logger.Info("WebHost is started");

    try
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BaseGenerateContext>();
        await LogDatabaseTriggersAsync(context, logger);
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Не удалось прочитать триггеры базы");
    }

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "WebHost can`t started");
}

static async Task LogDatabaseTriggersAsync(BaseGenerateContext context, Logger logger)
{
    var connection = context.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
        await connection.OpenAsync();

    await using var command = connection.CreateCommand();
    command.CommandText = """
        SELECT TRIGGER_NAME, EVENT_OBJECT_TABLE, ACTION_TIMING, EVENT_MANIPULATION, ACTION_STATEMENT
        FROM information_schema.TRIGGERS
        WHERE TRIGGER_SCHEMA = DATABASE()
        """;

    var found = false;
    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        found = true;
        var statement = reader.GetString(4).Replace("\r", " ").Replace("\n", " ");
        logger.Warn(
            $"В базе найден триггер {reader.GetString(0)} на таблице {reader.GetString(1)}: {reader.GetString(2)} {reader.GetString(3)}. {statement}");
    }

    if (!found)
        logger.Info("Триггеров в базе нет.");
}