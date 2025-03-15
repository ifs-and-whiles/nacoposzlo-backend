using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Billy.IntegrationTests.Infrastructure.Api
{
    public class ApiMock : IDisposable
    {
        private readonly string _url;
        private readonly Endpoint[] _endpoints;
        public List<string> RequestedUrls { get; } = new List<string>();

        private IWebHost _host;

        public ApiMock(string url, params Endpoint[] endpoints)
        {
            _url = url;
            _endpoints = endpoints;
        }

        public void Start()
        {
            _host = new WebHostBuilder()
               .UseKestrel()
               .UseUrls(_url)
               .ConfigureServices(s => s.AddRouting())
               .Configure(app =>
               {
                   foreach (var endpoint in _endpoints)
                   {
                       app.UseRouter(r =>
                       {
                           if (endpoint.HttpMethod == HttpMethod.Get)
                           {
                               r.Routes.Add(r.MapGet(endpoint.Url,
                                   async (request, response, routeData) =>
                                   {
                                       response.StatusCode = (int)endpoint.HttpCode;

                                       AssignHeaders(endpoint, response);

                                       await response.WriteAsync(endpoint.Result);

                                   }).Build());
                           }
                           else if (endpoint.HttpMethod == HttpMethod.Post)
                           {
                               r.Routes.Add(r.MapPost(endpoint.Url,
                                   async (request, response, routeData) =>
                                   {
                                       response.StatusCode = (int)endpoint.HttpCode;
                                       
                                       AssignHeaders(endpoint, response);

                                       await response.WriteAsync(endpoint.Result);

                                   }).Build());
                           }
                       });
                   }
               })
               .Build();

            _host.Start();
        }

        private static void AssignHeaders(Endpoint endpoint, HttpResponse response)
        {
            if (endpoint.Headers != null)
            {
                foreach (var header in endpoint.Headers)
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }

    public class Endpoint
    {
        public HttpMethod HttpMethod { get; set; }
        public string Url { get; set; }
        public string Result { get; set; }
        public HttpStatusCode HttpCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; set; }
    }

    public enum HttpMethod
    {
        Get,
        Post
    }
}
