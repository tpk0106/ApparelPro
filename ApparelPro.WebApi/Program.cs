using ApparelPro.Data;
using ApparelPro.Data.Models.Registration;
using ApparelPro.WebApi.Extensions;
using ApparelPro.WebApi.Misc;
using Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Text.Json;
using static ApparelPro.WebApi.Misc.ByteArrayConverter;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Global startup diagnostics ─────────────────────────────────────
try
{
Console.WriteLine("=== ApparelPro Backend Starting ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Connection string present: {!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("ApparelProConnection"))}");
Console.WriteLine($"JwtSettings:TokenKey present: {!string.IsNullOrEmpty(builder.Configuration["JwtSettings:TokenKey"])}");
Console.WriteLine($"JwtSettings:Issuer present: {!string.IsNullOrEmpty(builder.Configuration["JwtSettings:Issuer"])}");
Console.WriteLine($"Cors:Origins value: {builder.Configuration["Cors:Origins"] ?? "(null)"}");

// Add Serilog support — gracefully skip MSSQL sink when connection string is absent
builder.Host.UseSerilog((ctx, lc) =>
{
    var connStr = ctx.Configuration.GetConnectionString("ApparelProConnection");

    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console();

    // Only add the MSSQL sink if we actually have a connection string
    if (!string.IsNullOrEmpty(connStr))
    {
        lc.WriteTo.MSSqlServer(
            connectionString: connStr,
            restrictedToMinimumLevel: LogEventLevel.Information,
            sinkOptions: new MSSqlServerSinkOptions { TableName = "LogEvents", AutoCreateSqlTable = true }
        );

        // In Serilog configuration -- filter EF Core command events by duration
        lc.WriteTo.Logger(sub => sub
            .Filter.ByIncludingOnly(e =>
                e.Properties.TryGetValue("ElapsedMilliseconds", out var ms) &&
                ms is ScalarValue sv &&
                sv.Value is long ms2 &&
                ms2 > 500)
            .WriteTo.File("logs/slow-queries-.log", rollingInterval: RollingInterval.Day));
    }
    else
    {
        Console.WriteLine("WARNING: ApparelProConnection is null — Serilog MSSQL sink disabled, logging to console only.");
    }
});

//// Add ASP.NET Core Identity support
builder.Services.AddIdentity<ApparelProUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
}).AddEntityFrameworkStores<UserIdentityDbContext>();

// Add services to the container.

ServiceExtensions.ConfigureApparelProDatabase(builder.Services, builder.Configuration, builder.Environment.IsDevelopment());
ServiceExtensions.ConfigureApparelProIdentity(builder.Services, builder.Configuration);

ServiceExtensions.ConfigureApparelProServices(builder.Services);

ServiceExtensions.ConfigureApparelProOrderManagementServices(builder.Services);

ServiceExtensions.ConfigureParamsData(builder.Configuration);

ServiceExtensions.ConfgureAppsettings(builder.Services, builder.Configuration);

//builder.Services.AddDbContextPool<ApparelProDbContext>(options => options.UseSqlServer())

// added by thusith on 18/03/2024 due to CreatedAtAction error 
//ref : https://www.josephguadagno.net/2020/07/01/no-route-matches-the-supplied-values
ServiceExtensions.UpdateMvcOptions(builder.Services);

// This handles all AutoMapper v15/v16 assemblies concurrently without breaking
builder.Services.AddAutoMapper(cfg =>
{
    // Scans every profile class within the current execution space
    cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
});


// Register your open generic type converter so DI can instantiate it
//builder.Services.AddTransient(typeof(PaginationResultToPaginationAPITypeConverter<,>));

//builder.Services.AddAutoMapper(cfg =>
//{
//    // Scans the assembly containing DatabaseToServiceMappings
//    cfg.RegisterServicesFromAssembly(typeof(apparelPro.BusinessLogic.Services.Mappings.DatabaseToServiceMappings).Assembly);

//    // Scans the assembly containing ServicetoAPIModelMappings
//    cfg.RegisterServicesFromAssembly(typeof(ApparelPro.WebApi.Mappings.ServicetoAPIModelMappings).Assembly);
//});


//IServiceCollection serviceCollection = builder.Services.AddAutoMapper(AppDcd omain.CurrentDomain.GetAssemblies());

//builder.Services.AddScoped<ValidationFilterAttribute, ValidationFilterAttribute>();

// authorization
ServiceExtensions.AddAuthorization(builder.Services, builder.Configuration);

// Stage 2 access-control groundwork (see ApparelPro.WebApi/Authorization/) - registers
// the dynamic, permission-table-backed IAuthorizationPolicyProvider/IAuthorizationHandler.
// No controller uses this yet; that cutover is a deliberately separate, later pass.
ServiceExtensions.ConfigurePermissionAuthorizationInfrastructure(builder.Services);

// authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(opt =>
{
    // .NET 8+ switched the default JWT bearer token handler to JsonWebTokenHandler,
    // which - unlike the legacy JwtSecurityTokenHandler - does NOT remap short wire
    // claim names ("role", "unique_name") back to the long ClaimTypes URIs that
    // [Authorize(Roles = "...")] / User.IsInRole(...) actually check against. Without
    // this, EVERY role-based authorization check in the app silently fails, even for
    // a token that correctly contains the required roles (confirmed via decoded JWT:
    // "role": ["Store Manager", "Administrator"] still got rejected before this fix).
    opt.MapInboundClaims = true;
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        RequireExpirationTime = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:TokenKey"]!))
    };
    opt.IncludeErrorDetails = true;
});

// handle CORS
// https://stackoverflow.com/questions/42199757/enable-options-header-for-cors-on-net-core-web-api

const string reactPolicyName = "allowFromReactOrigin";
const string angularPolicyName = "allowFromAngularOrigin";

string?  coreOrigin  = builder.Configuration["Cors:Origins"];
builder.Services.AddCors(options =>
{
    options.AddPolicy(reactPolicyName, builder =>
    {
        builder
        //.WithOrigins("https://localhost:5173", "http://localhost:5174")
        //.WithOrigins("http://localhost:3000") // put react url frm the browser

        .WithOrigins((coreOrigin ?? "http://localhost:3000")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
)

        //.WithMethods("DELETE","PUT","GET", "POST")    
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });

    options.AddPolicy(angularPolicyName, builder =>
    {
        builder
        .WithOrigins("http://localhost:4200")  // put angular url
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var serializerOptions = new JsonSerializerOptions()
{
    Converters = { new JsonToByteArrayConverter() }
};

// add converters

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonToByteArrayConverter()));
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new EverythingToStringJsonConverter()));

//var options = new JsonSerializerOptions()
//{
//    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling | System.Text.Json.Serialization.JsonNumberHandling.WriteAsString
//};

//builder.Services.AddControllers().AddJsonOptions(options);

//builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

//builder.Services.AddControllers().AddJsonOptions(options =>
//    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter()));

// add cache profiles
builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add("No-Cache", new CacheProfile() { NoStore = true });
    options.CacheProfiles.Add("Any-60", new CacheProfile() { Location = ResponseCacheLocation.Any, Duration = 60 });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // 1. Define the security scheme natively
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token to login to ApparelPro",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    // 2. Map the requirement using the dedicated .NET 10 OpenApiSecuritySchemeReference class
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});


//builder.Services.AddSwaggerGen(options =>
//{
//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
//    {
//        In = ParameterLocation.Header,
//        Description = "Please enter token to login to ApparelPro",
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        BearerFormat = "JWT",
//        Scheme = "bearer"
//    });

//    // 2. Updated .NET 10 syntax separating the scheme and reference initialization
//    var securityScheme = new OpenApiSecurityScheme
//    {
//        Reference = new OpenApiReference()
//        {
//            Type = ReferenceType.SecurityScheme,
//            Id = "Bearer"
//        }
//    };

//    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
//    {
//        { securityScheme, Array.Empty<string>() }
//    });

//    //options.AddSecurityRequirement(new OpenApiSecurityRequirement()
//    //{
//    //    {
//    //        new OpenApiSecurityScheme()
//    //        {

//    //            Reference = new BaseOpenApiReference()
//    //            {
//    //                Type = ReferenceType.SecurityScheme,
//    //                Id="Bearer"
//    //            }
//    //        },
//    //        Array.Empty<string>()
//    //    }
//    //});
//});


// add response caching middleware for server side caching
builder.Services.AddResponseCaching();

var cacheConnStr = builder.Configuration.GetConnectionString("ApparelProConnection");
if (!string.IsNullOrEmpty(cacheConnStr))
{
    builder.Services.AddDistributedSqlServerCache(options =>
    {
        options.ConnectionString = cacheConnStr;
        options.SchemaName = "dbo";
        options.TableName = "AppCache";
    });
}
else
{
    // Fallback to in-memory distributed cache when no SQL connection available
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("WARNING: Using in-memory distributed cache (no SQL connection string).");
}

var app = builder.Build();

// ── One-time admin seed ─────────────────────────────────────────────
try
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApparelProUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Seed roles
        string[] roles = { "Administrator", "Inventory", "Merchandiser", "Merchandiser Manager", "Store Supervisor",
                       "Order Entry Operator", "Store Manager", "Production Manager", "Factory Manager", "Board Of Directors" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed admin user
        string adminEmail = "admin@apparelpro.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApparelProUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                KnownAs = "System Admin",
                Gender = Gender.Male,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@apparelpro66");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Seed error: {ex.Message}");
}
// ── End seed ────────────────────────────────────────────────────────

// serilog middleware
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // 2. Point Scalar directly to your working Swashbuckle JSON URL
 
    // run in the browser wth "https://localhost:5000/Scalar/V1"

    app.MapScalarApiReference(options =>
    {
        // 1. Maintain your Swashbuckle route mapping
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");

        // 2. Inject your custom CSS theme variables into the document head
        options.WithCustomCss(@"
            .light-mode .sidebar {
              --scalar-sidebar-background-1: var(--scalar-background-1);
              --scalar-sidebar-item-hover-color: currentColor;
              --scalar-sidebar-item-hover-background: var(--scalar-background-2);
              --scalar-sidebar-item-active-background: var(--scalar-background-2);
              --scalar-sidebar-border-color: var(--scalar-border-color);
              --scalar-sidebar-color-1: var(--scalar-color-1);
              --scalar-sidebar-color-2: var(--scalar-color-2);
              --scalar-sidebar-color-active: var(--scalar-color-2);
              --scalar-sidebar-search-background: var(--scalar-background-2);
              --scalar-sidebar-search-border-color: var(--scalar-border-color);
              --scalar-sidebar-search-color: var(--scalar-color-3);
            }
            .dark-mode .sidebar {
              --scalar-sidebar-background-1: var(--scalar-background-1);
              --scalar-sidebar-item-hover-color: currentColor;
              --scalar-sidebar-item-hover-background: var(--scalar-background-2);
              --scalar-sidebar-item-active-background: var(--scalar-background-2);
              --scalar-sidebar-border-color: var(--scalar-border-color);
              --scalar-sidebar-color-1: var(--scalar-color-1);
              --scalar-sidebar-color-2: var(--scalar-color-2);
              --scalar-sidebar-color-active: var(--scalar-color-2);
              --scalar-sidebar-search-background: var(--scalar-background-2);
              --scalar-sidebar-search-border-color: var(--scalar-border-color);
              --scalar-sidebar-search-color: var(--scalar-color-3);
            }
        ");
    });

}

// 1. MUST call UseCors FIRST and explicitly pass your policy name!
app.UseCors(reactPolicyName);
//app.UseOptions();
app.Use(async (context, next) =>
{

    // 2. Disable caching if required
    context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
    {
        NoCache = true,
        NoStore = true
    };

    //(Note: You can safely delete that entire chunk of custom manual
    //context.Response.Headers["Access-Control-Allow-Origin"] = ... code completely).

    // // enable for react apps below

    // //context.Response.Headers["Access-Control-Allow-Origin"] = "https://localhost:5173";
    // context.Response.Headers["Access-Control-Allow-Origin"] = "http://localhost:3000";

    // // end of enable

    // // enable for angular apps below

    ////  context.Response.Headers["Access-Control-Allow-Origin"] = "http://localhost:4200";

    // // end of enable

    // context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, DELETE, PUT, OPTIONS";

    // context.Response.Headers.Allow = "GET, POST, DELETE, PUT, OPTIONS";
    // if (HttpMethods.IsOptions(context.Request.Method))
    // {        
    //     // below is working for react
    //     //context.Response.Headers["Access-Control-Allow-Methods"] = " DELETE";
    //     // added by thusith on 20/02/25 to test 
    //     context.Response.Headers["Access-Control-Allow-Methods"] = " DELETE, PUT, POST, GET";
    //     // end of react settings

    //     // change for angular 
    // //    context.Response.Headers["Access-Control-Allow-Methods"] = "DELETE, PUT, POST";
    //     // end of change
    //     context.Response.Headers["Access-Control-Allow-Headers"] = "X-Requested-With, Accept, Access-Control-Allow-Origin, Content-Type, Authorization";
    //     await context.Response.CompleteAsync();
    //     return;
    // }
    await next();

});
// Only redirect to HTTPS in development; Traefik handles SSL termination in production
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 4. Run Authentication and Authorization AFTER CORS
app.UseAuthentication(); // Ensure this is present if using JWT!

app.UseAuthorization();

app.MapControllers();

// Temporary startup script to auto-generate the missing database file
//using (var scope = app.Services.CreateScope())
//{
//    var identityContext = scope.ServiceProvider.GetRequiredService<UserIdentityDbContext>();
//    var apparelContext = scope.ServiceProvider.GetRequiredService<ApparelProDbContext>();

//    // This instructs SQL Express to create the database file and all your missing tables
//    await identityContext.Database.EnsureCreatedAsync();
//    await apparelContext.Database.EnsureCreatedAsync();
//}
Console.WriteLine("=== ApparelPro Backend ready, starting listener ===");
app.Run();

}
catch (Exception ex)
{
    Console.Error.WriteLine("========================================");
    Console.Error.WriteLine("FATAL: Application startup crashed!");
    Console.Error.WriteLine($"Exception: {ex.GetType().Name}");
    Console.Error.WriteLine($"Message: {ex.Message}");
    Console.Error.WriteLine($"StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.Error.WriteLine($"Inner: {ex.InnerException.Message}");
        Console.Error.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
    }
    Console.Error.WriteLine("========================================");
    Console.Error.Flush();
    Console.Out.Flush();
    // Keep container alive briefly so logs can be captured
    await Task.Delay(30000);
    Environment.Exit(1);
}
