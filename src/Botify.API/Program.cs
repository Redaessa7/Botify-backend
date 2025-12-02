using Botify.API.Filters;
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
    .ConfigureJsonConverters();

builder.Services
    .AddPresentation(builder.Configuration, builder.Environment)
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

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
app.MapFallbackToFile("index.html");

app.Run();
