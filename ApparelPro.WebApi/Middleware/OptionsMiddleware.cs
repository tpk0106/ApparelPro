namespace ApparelPro.WebApi.Middleware
{
    public class OptionsMiddleware
    {
        private readonly RequestDelegate _next;
        public OptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }      

        public Task Invoke(HttpContext context)
        {
            return BeginInvoke(context);
        }

        private Task BeginInvoke(HttpContext context)
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*" );
                //context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)context.Request.Headers["Origin"] });
              //  context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept" });
                context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "DELETE, OPTIONS, POST" });
            //    context.Response.Headers.Add("Allow", "DELETE");
                context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
                context.Response.StatusCode = 200;
                return context.Response.WriteAsync("OK");
            }
          
            return _next.Invoke(context);
        }

    }
}
