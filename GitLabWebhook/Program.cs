using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using GitLabWebhook;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add Swagger services
builder.Services.AddSwaggerGen(); // Adds Swagger generator

// Load the configuration from appsettings.json
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", optional: false, reloadOnChange: true);

// If you have other services like a database context, you can add them here:
// builder.Services.AddDbContext<ApplicationDbContext>(options => ...);
// builder.Services.AddScoped<IMyService, MyService>();

// You can also add other configurations, e.g., logging, authentication, etc.
// builder.Services.AddLogging();
// builder.Services.AddAuthentication(...);
// builder.Services.AddAuthorization();


builder.Services.RegisterIocConfigurations(builder.Environment, builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline (middleware).
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Show detailed errors in development.
    app.UseSwagger(); // Enable Swagger UI
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"); // Setup the Swagger endpoint
        c.RoutePrefix = "swagger"; // Makes Swagger UI available at the root
    });
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Show generic errors in production.
    app.UseHsts(); // Strict Transport Security
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "images")),
    RequestPath = "/images"
});

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS.
app.UseRouting(); // Enable routing.

app.MapControllers(); // Map controller routes (for API or MVC controllers).

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run(); // Start the application.
