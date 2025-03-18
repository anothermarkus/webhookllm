using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// This is where you add services like controllers, databases, etc.
builder.Services.AddControllers();

// If you have other services like a database context, you can add them here:
// builder.Services.AddDbContext<ApplicationDbContext>(options => ...);
// builder.Services.AddScoped<IMyService, MyService>();

// You can also add other configurations, e.g., logging, authentication, etc.
// builder.Services.AddLogging();
// builder.Services.AddAuthentication(...);
// builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline (middleware).
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Show detailed errors in development.
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Show generic errors in production.
    app.UseHsts(); // Strict Transport Security
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS.
app.UseRouting(); // Enable routing.

app.MapControllers(); // Map controller routes (for API or MVC controllers).

app.Run(); // Start the application.
