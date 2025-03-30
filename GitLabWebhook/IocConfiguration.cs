
namespace GitLabWebhook
{

    /// <summary>Registers IoC configurations.</summary>
    public static class IocConfiguration
    {
                
        /// <summary>Registers IoC configurations.</summary>
        public static IServiceCollection RegisterIocConfigurations(this IServiceCollection services, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
        
            services.AddHttpClient();
        
        // services.AddTransient<GitLabService>(services => new GitLabService(services.GetRequiredService<IConfiguration>(), services.GetRequiredService<IHttpClientFactory>()));
        
            return services;
        }
        
    }
}
