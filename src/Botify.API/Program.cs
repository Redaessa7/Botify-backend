using System.Text.Json.Serialization;
using Botify.API.CrossCutting.Filters;
using Botify.API.CrossCutting.Setup;
using Botify.API.Validators;
using FluentValidation;

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

// Seed roles
using (var scope = app.Services.CreateScope())
{
    await InfrastructureRegisterServices.SeedRolesAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseCoreMiddlewares(builder.Configuration);

app.MapControllers();

app.Run();
