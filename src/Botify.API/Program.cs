using Botify.API.CrossCutting.Constants;
using Botify.API.CrossCutting.Extensions;
using Botify.API.CrossCutting.Setup;
using Scalar.AspNetCore;

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

#region DI Container
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

app.MapControllers();

#endregion

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.SeedRolesAsync();
}

app.Run();
