namespace ApparelPro.WebApi.Middleware
{
    public static class OptionsMiddlewareExtension
    {       
       public static IApplicationBuilder UseOptions(this IApplicationBuilder builder)
       {
           return builder.UseMiddleware<OptionsMiddleware>();
       }       
    }
}
