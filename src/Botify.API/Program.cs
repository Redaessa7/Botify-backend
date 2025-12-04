using System.Text.Json.Serialization;
using Botify.API.CrossCutting.Filters;
using Botify.API.CrossCutting.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(builder.Environment.WebRootPath);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<EnvelopeResultFilter>();
        //options.Filters.Add<FluentValidationFilter>();
    })
    .AddJsonOptions(options =>{ options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

builder.Services.AddApplicationSetup(builder.Configuration, builder.Environment);



var app = builder.Build();
app.UseExceptionHandler();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.SeedRolesAsync();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCoreMiddlewares(builder.Configuration);

app.UseAuthentication();
app.UseAuthorization();

app.Run();
