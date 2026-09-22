using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Providers;
using ApparelPro.AI.Services;

namespace ApparelPro.AI;

/// <summary>
/// Extension method to register all ApparelPro.AI services.
/// Call builder.Services.AddApparelProAI(builder.Configuration) in Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApparelProAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<AiSettings>(options =>
     configuration.GetSection(AiSettings.SectionName).Bind(options));

        // Register BOTH providers so voice mode can override to OpenAI at runtime.
        // The AiService selects the active one based on config or the preferredProvider parameter.
        services.AddSingleton<IAiProvider, AnthropicAiProvider>();
        services.AddSingleton<IAiProvider, OpenAiProvider>();

        // Register the core AI services
        services.AddSingleton<IAiService, AiService>();
        services.AddScoped<IAiChatService, AiChatService>();

        return services;
    }
}
