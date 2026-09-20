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

        // Register both providers — the AiService selects the active one at runtime
        //services.AddSingleton<IAiProvider, AnthropicAiProvider>();
        //services.AddSingleton<IAiProvider, OpenAiProvider>();


        //var aiSection = new AiSettings();
        var aiSection = new AiSettings { ActiveProvider = "Anthropic" };
        configuration.GetSection(AiSettings.SectionName).Bind(aiSection);

        //var activeProvider = aiSection.ActiveProvider?.ToUpperInvariant() ?? "ANTHROPIC";

        var activeProvider = configuration
            .GetSection(AiSettings.SectionName)["ActiveProvider"]?
            .ToUpperInvariant() ?? "ANTHROPIC";

        if (activeProvider == "ANTHROPIC")
            services.AddSingleton<IAiProvider, AnthropicAiProvider>();
        else
            services.AddSingleton<IAiProvider, OpenAiProvider>();


        // Register the core service
        services.AddSingleton<IAiService, AiService>();

        return services;
    }
}
