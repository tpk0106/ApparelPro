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
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Text.Json;
using static ApparelPro.WebApi.Misc.ByteArrayConverter;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog support
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.MSSqlServer(
        connectionString: ctx.Configuration.GetConnectionString("ApparelProConnection"),
        restrictedToMinimumLevel: LogEventLevel.Information,
        sinkOptions: new MSSqlServerSinkOptions { TableName = "LogEvents", AutoCreateSqlTable = true }
        )
    .WriteTo.Console()
);

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

ServiceExtensions.ConfigureApparelProDatabase(builder.Services, builder.Configuration);
ServiceExtensions.ConfigureApparelProIdentity(builder.Services, builder.Configuration);

ServiceExtensions.ConfigureApparelProServices(builder.Services);

ServiceExtensions.ConfigureApparelProOrderManagementServices(builder.Services);

ServiceExtensions.ConfigureParamsData(builder.Configuration);

ServiceExtensions.ConfgureAppsettings(builder.Services, builder.Configuration);

//builder.Services.AddDbContextPool<ApparelProDbContext>(options => options.UseSqlServer())

// added by thusith on 18/03/2024 due to CreatedAtAction error 
//ref : https://www.josephguadagno.net/2020/07/01/no-route-matches-the-supplied-values
ServiceExtensions.UpdateMvcOptions(builder.Services);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//builder.Services.AddScoped<ValidationFilterAttribute, ValidationFilterAttribute>();

// authorization
ServiceExtensions.AddAuthorization(builder.Services, builder.Configuration);

// authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(opt =>
{
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
builder.Services.AddCors(options =>
{
options.AddPolicy(reactPolicyName, builder =>
{
    builder
    //.WithOrigins("https://localhost:5173", "http://localhost:5174")
    .WithOrigins("http://localhost:3000") // put react url frm the browser
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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "Please enter token to login to ApparelPro",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// add response caching middleware for server side caching
builder.Services.AddResponseCaching();

builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApparelProConnection");
    options.SchemaName = "dbo";
    options.TableName = "AppCache";
});

var app = builder.Build();

// serilog middleware
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
app.UseHttpsRedirection();

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


app.Run();
