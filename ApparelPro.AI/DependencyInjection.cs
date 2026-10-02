using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.BackgroundJobs;
using ApparelPro.AI.Configuration;
using ApparelPro.AI.Providers;
using ApparelPro.AI.Services;
using apparelPro.BusinessLogic.Services.Implementation.AI;
using apparelPro.BusinessLogic.Services;

namespace ApparelPro.AI;

/// <summary>
/// Extension method to register all ApparelPro.AI services.
/// Call builder.Services.AddApparelProAI(builder.Configuration) in Program.cs.
///
/// 🎓 WHAT CHANGED FOR RAG:
/// We added 6 new registrations (marked with 🆕):
///   1. RagSettings configuration binding
///   2. IEmbeddingService → EmbeddingService (singleton)
///   3. IVectorStoreService → QdrantVectorStoreService (singleton)
///   4. EntityChunkerService (singleton)
///   5. EmbeddingSyncJob (hosted background service)
///   6. IRagService → RagService (singleton) — the query pipeline orchestrator
///
/// 🎓 WHY SINGLETON for RAG services?
/// - EmbeddingService: Holds an OpenAI EmbeddingClient that's thread-safe and reusable
/// - QdrantVectorStoreService: Holds a QdrantClient with a connection pool
/// - EntityChunkerService: Stateless — no reason to create new instances per request
///
/// These are different from AiChatService (scoped) because the chat service
/// tracks per-request state (session IDs). RAG services are stateless utilities.
///
/// 🎓 WHY AddHostedService for EmbeddingSyncJob?
/// AddHostedService registers a BackgroundService that:
///   - Starts automatically when the application starts
///   - Runs on its own thread (doesn't block API requests)
///   - Stops gracefully on application shutdown
///   - Is managed by the .NET hosting infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApparelProAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Configuration binding ────────────────────
        // 🎓 Bind "AiSettings" section from appsettings.json
        services.Configure<AiSettings>(options =>
             configuration.GetSection(AiSettings.SectionName).Bind(options));

        // 🆕 Bind "AiSettings:Rag" section from appsettings.json
        // 🎓 WHY A SUB-SECTION?
        // RagSettings lives under AiSettings in the config hierarchy:
        //   {
        //     "AiSettings": {
        //       "ActiveProvider": "Anthropic",
        //       "Anthropic": { ... },
        //       "OpenAI": { ... },
        //       "Rag": {                          ← This section
        //         "QdrantUrl": "http://qdrant:6333",
        //         "CollectionName": "apparelpro-entities",
        //         "EmbeddingModel": "text-embedding-3-small",
        //         ...
        //       }
        //     }
        //   }
        services.Configure<RagSettings>(options =>
            configuration.GetSection($"{AiSettings.SectionName}:Rag").Bind(options));

        // ── AI Providers (existing) ──────────────────
        // Register BOTH providers so voice mode can override to OpenAI at runtime.
        // The AiService selects the active one based on config or the preferredProvider parameter.
        services.AddSingleton<IAiProvider, AnthropicAiProvider>();
        services.AddSingleton<IAiProvider, OpenAiProvider>();

        // ── Core AI services (existing) ──────────────
        services.AddSingleton<IAiService, AiService>();
        services.AddScoped<IAiChatService, AiChatService>();

        // ── 🆕 RAG services ─────────────────────────
        // 🎓 EMBEDDING SERVICE:
        // Converts text → vectors using OpenAI's embedding API.
        // Singleton because the OpenAI client is thread-safe and reusable.
        services.AddSingleton<IEmbeddingService, EmbeddingService>();

        // 🎓 VECTOR STORE SERVICE:
        // Stores and searches vectors in Qdrant.
        // Singleton because the Qdrant client maintains a connection pool.
        services.AddSingleton<IVectorStoreService, QdrantVectorStoreService>();

        // 🎓 ENTITY CHUNKER:
        // Converts ERP entities into text chunks for embedding.
        // Singleton because it's stateless (no dependencies that change per request).
        services.AddSingleton<EntityChunkerService>();

        // 🆕 🎓 RAG QUERY PIPELINE:
        // Orchestrates the full RAG flow: embed question → search Qdrant → build context → Claude.
        // Singleton because it's stateless — all its dependencies are also singletons.
        // This is the service that RagController calls to answer cross-entity questions.
        services.AddSingleton<IRagService, RagService>();

        // 🎓 BACKGROUND SYNC JOB:
        // Periodically syncs entity data into the vector store.
        // AddHostedService registers it as a long-running background task.
        // The job uses IServiceScopeFactory internally to create scoped DbContext instances.
        services.AddHostedService<EmbeddingSyncJob>();

        //     // 🎓 ReportRegistry: Read-only catalogue of RAG-detectable reports.
        //     // Registered as Transient — same pattern as BuyerService and other
        //     // reference services. Transient is fine because:
        //     //   • No expensive state to initialise
        //     //   • Small service with simple DB queries
        //     //   • ApparelProDbContext (scoped) is safely consumed by transient
        services.AddTransient(typeof(IReportRegistryService), typeof(ReportRegistryService));
        //
        // 🎓 SOP (Standard Operating Procedures) — Phase 2.
        // Registered as Transient — same pattern as ReportRegistryService.
        // SopService provides full CRUD for SOPs and the context-aware query
        // used by the PDF engine to inject SOPs into reports.
        services.AddTransient(typeof(ISopService), typeof(SopService));

        //     // 🎓 Future AI services will be added here:
       //  services.AddTransient(typeof(IReportIntentService), typeof(ReportIntentService));

        return services;
    }
}
