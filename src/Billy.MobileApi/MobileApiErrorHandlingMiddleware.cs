using System;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Billy.CQRS;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;

namespace Billy.MobileApi
{
    public class MobileApiErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ILogger Logger = Log.ForContext<MobileApiErrorHandlingMiddleware>();
        
        public MobileApiErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments($"/{MobileApiRouteConsts.Prefix}"))
            {
                try
                {
                    await _next(context);
                }
                catch (NotFoundException exception)
                {
                    LogWarningWithRequestInfo(exception, context);

                    await HandleNotFoundExceptionAsync(context);
                }
                catch (Exception exception)
                {
                    LogErrorWithRequestInfo(exception, context);

                    await HandleExceptionAsync(context);
                }
            }
            else
                //Skip processing if request does to belong to Mobile API 
                await _next(context);
            
        } 
        
        private void LogErrorWithRequestInfo(Exception exception, HttpContext context)
        {
            Logger.Error(exception, exception.Message + " Request: {@RequestContext}", new
            {
                context?.Request?.Protocol,
                context?.Request?.Method,
                context?.Request?.Path,
                context?.Request?.QueryString
            });
        }

        private void LogWarningWithRequestInfo(Exception exception, HttpContext context)
        {
            Logger.Warning(exception, exception.Message + " Request: {@RequestContext}", new
            {
                context?.Request?.Protocol,
                context?.Request?.Method,
                context?.Request?.Path,
                context?.Request?.QueryString
            });
        }

        private Task HandleExceptionAsync(HttpContext context)
        {
            var code = HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Server error occured, please contact with administrator."
            }.ToString());

        }
        
        private Task HandleNotFoundExceptionAsync(HttpContext context)
        {
            var code = HttpStatusCode.NotFound;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Requested element not found"
            }.ToString());
        }
    }
    
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}