
using GitLabWebhook.CodeReviewServices.Strategies;

namespace GitLabWebhook
{

    /// <summary>Registers IoC configurations.</summary>
    public static class IocConfiguration
    {
                
        /// <summary>Registers IoC configurations.</summary>
        public static IServiceCollection RegisterIocConfigurations(this IServiceCollection services, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
        
            services.AddHttpClient();
            services.AddScoped<IPromptGenerationStrategy, FewShotPromptGenerationStrategy>();
            services.AddScoped<IPromptGenerationStrategy, ZeroShotPromptGenerationStrategy>();
            // TODO implement and add other prompt generation strategies

            // Registered services will automagically add to the cosntructor
            services.AddScoped<IPromptGenerationStrategyFactory, PromptGenerationStrategyFactory>();
            //servies.AddScoped<PromptGenerationStrategyFactory>();


        
 
            return services;
        }
        
    }
}
