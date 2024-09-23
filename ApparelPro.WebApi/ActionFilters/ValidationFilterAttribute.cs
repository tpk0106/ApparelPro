using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

//https://code-maze.com/action-filters-aspnetcore/
//https://codereview.stackexchange.com/questions/123137/custom-web-api-asynchronous-filter-implementation

namespace ApparelPro.WebApi.ActionFilters
{
    public class ValidationFilterAttribute : FilterAttribute, IActionFilter
    {
//        public bool AllowMultiple => throw new NotImplementedException();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public bool AllowMultiple => false;


        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var param = actionContext.ActionArguments
                .SingleOrDefault(p=>p.Value is EntityEntry entityEntry);

            await InternalActionExecuting(actionContext);

            if (actionContext.Response != null)
                return actionContext.Response;

            HttpActionExecutedContext executedContext;
            try
            {
                var response = await continuation();
                executedContext = new HttpActionExecutedContext(actionContext, null)
                {
                    Response = response
                };
            }
            catch (Exception exception)
            {
                executedContext = new HttpActionExecutedContext(actionContext, exception);
            }

            await InternalActionExecuted(executedContext);
            _stopwatch.Reset();

            return executedContext.Response;
        }
        private Task InternalActionExecuting(HttpActionContext actionContext)
        {
            _stopwatch.Start();
            return Log("Executing", actionContext, 0);
        }

        private Task InternalActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            _stopwatch.Stop();
            return Log("Executed", actionExecutedContext.ActionContext, _stopwatch.ElapsedMilliseconds);
        }
        private Task Log(string action, HttpActionContext actionContext, long elapsedTime)
        {
            return Task.Run(() =>
            {
                var controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
                var actionName = actionContext.ActionDescriptor.ActionName;
                var parameters = string.Join(", ", actionContext.ActionArguments.Values.Select(x => x).ToArray());

                var message = $"{action}: ctrl: {controllerName}, act: {actionName}, params: {parameters}" +
                              $"{(elapsedTime > 0 ? "took (ms): " + elapsedTime : string.Empty)}";

                Trace.WriteLine(message, "Action filter log");
            });
        }
    }
}
