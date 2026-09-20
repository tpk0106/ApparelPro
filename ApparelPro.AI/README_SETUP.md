# ApparelPro.AI — Setup Guide

## 1. Add the project to your solution

```bash
cd your-solution-folder
dotnet sln add ApparelPro.AI/ApparelPro.AI.csproj
```

## 2. Reference it from your main API project

```bash
cd ApparelPro.API
dotnet add reference ../ApparelPro.AI/ApparelPro.AI.csproj
```

## 3. Register services in Program.cs

Add this single line after your existing service registrations:

```csharp
builder.Services.AddApparelProAI(builder.Configuration);
```

## 4. Add configuration to appsettings.json

Merge the contents of `appsettings.AI.json` into your main `appsettings.json`.

**IMPORTANT**: Store API keys in User Secrets (development) or environment variables (production):

```bash
# Development — User Secrets
dotnet user-secrets set "AiSettings:Anthropic:ApiKey" "sk-ant-..."
dotnet user-secrets set "AiSettings:OpenAI:ApiKey" "sk-..."

# Production — Environment Variables (Coolify)
AiSettings__Anthropic__ApiKey=sk-ant-...
AiSettings__OpenAI__ApiKey=sk-...
```

## 5. Move the controller

Copy `Controller/AiController.cs` into your main API project's `Controllers/` folder.
Update the namespace if needed to match your API project.

## 6. Switch providers

Change `"ActiveProvider"` in config to `"Anthropic"` or `"OpenAI"`.
No code changes needed — the service routes to whichever is active.

## 7. Wire up real entity data

In `AiController.SummariseEntityAsync`, replace the placeholder with actual
entity lookups from your existing services. Example:

```csharp
var style = await _styleService.GetStyleDetailsAsync(request.EntityKey, cancellationToken);
var entityData = JsonSerializer.Serialize(style, new JsonSerializerOptions 
{ 
    WriteIndented = true 
});
```

## Project Structure

```
ApparelPro.AI/
├── Abstractions/
│   ├── IAiProvider.cs          # Provider interface (implement to add new providers)
│   └── IAiService.cs           # Service interface (inject this in controllers)
├── Configuration/
│   └── AiSettings.cs           # Strongly-typed config bound from appsettings
├── Controller/
│   └── AiController.cs         # Copy to your API project's Controllers/
├── Models/
│   ├── AiCompletionRequest.cs  # Internal completion request
│   ├── AiCompletionResponse.cs # Internal completion response with token tracking
│   ├── AiSummariseRequest.cs   # API-level request model
│   └── AiSummariseResponse.cs  # API-level response model
├── Prompts/
│   └── PromptTemplates.cs      # All prompt templates in one place
├── Providers/
│   ├── AnthropicAiProvider.cs  # Claude implementation
│   └── OpenAiProvider.cs       # GPT implementation
├── Services/
│   └── AiService.cs            # Core service — routes to active provider
├── DependencyInjection.cs      # One-line DI registration extension
├── ApparelPro.AI.csproj
└── appsettings.AI.json         # Config template (merge into your appsettings)
```
