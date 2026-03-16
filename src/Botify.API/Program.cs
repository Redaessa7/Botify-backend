using Botify.API.CrossCutting.Constants;
using Botify.API.CrossCutting.Extensions;
using Botify.API.CrossCutting.Setup;
using Botify.Application.CrossCutting.Options;
using Botify.Infrastructure.Persistence.Initialisation;
using Scalar.AspNetCore;
using Serilog;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

#region Bootstrap
builder.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), AppPaths.WwwRootFolder);
Directory.CreateDirectory(builder.Environment.WebRootPath);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(AppPaths.AppSettingsFile, optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
#endregion
builder.Services.AddOptions<PaginationOptions>()
    .Bind(builder.Configuration.GetSection("PaginationOptions"));
#region DI Container
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplicationSetup(builder.Configuration, builder.Environment);
#endregion

var app = builder.Build();

#region Middlewares

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(app.CurrentCorsPolicy().Name);
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseResponseCompression();
app.UseSerilogRequestLogging();
app.MapControllers();

#endregion

//using (var scope = app.Services.CreateScope())
//{
//    await scope.ServiceProvider.SeedRolesAsync();
//}
await app.InitialiseDatabaseAsync();

app.Run();
