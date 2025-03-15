using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Billy.EventStore;
using Billy.Infrastructure.Configs;
using Billy.Infrastructure.EventStore;
using Billy.Infrastructure.WebApp;
using Billy.IntegrationTests.Infrastructure.Database;
using Billy.IntegrationTests.Infrastructure.Utils;
using EventStore.ClientAPI;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Billy.IntegrationTests.Infrastructure
{
    public class HostFixture : IDisposable
    {
        public IHost Host { get; set; }
        public int ApiHostPort { get; } = 5000;
        public string HostAddress { get; }
        public IConfiguration AppConfiguration { get; }

        public HostFixture()
        {
            HostAddress = $"http://localhost:{ApiHostPort}";

            AppConfiguration = LoadConfig("appsettings-integration-tests.json");

            var dbConfig = AppConfiguration.GetSection("Database").Get<DatabaseConfig>();
            DatabaseCreator.CreateIfNotExists(dbConfig).Wait();

            Host = ApiHost.CreateHost(AppConfiguration, ApiHostPort);

            Host.Start();
        }

        private IConfiguration LoadConfig(string appSettings)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(appSettings, optional: false, false)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        public void Dispose()
        {
            Host?.StopAsync().Wait(millisecondsTimeout: 2000);
            Host?.Dispose();
        }
    }

    public abstract class TestFixture : IDisposable
    {
        public IHost Host { get; set; }
        public string HostAddress;
        private AuthenticationConfig _authenticationConfig;

        protected TestFixture(HostFixture hostFixture, ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(output)
                .CreateLogger();

            Host = hostFixture.Host;
            HostAddress = hostFixture.HostAddress;

            var projectionsHost = Host.Services.GetService<MartenProjectionsHost>();

            projectionsHost.StopAsync().Wait();
            
            SetupDatabase(hostFixture.AppConfiguration);
            
            projectionsHost.StartAsync().Wait();

            _authenticationConfig = hostFixture.AppConfiguration
                .GetSection(ConfigKeys.AuthenticationConfig)
                .Get<AuthenticationConfig>();
        }

        private void SetupDatabase(IConfiguration config)
        {
            var dbConfig = config.GetSection("Database").Get<DatabaseConfig>();

            Log.Debug("Test is using {ConnectionString}", dbConfig.ConnectionString);

            DatabaseCleanup.ClearDatabase(dbConfig.ConnectionString);

        }

        protected async Task<TResult> get_from_sut_api<TResult>(
            string path,
            params (string key, object value)[] queryParams)
        {
            var request = HostAddress.AppendPathSegment(path);
            queryParams.ForEach(p => request.SetQueryParam(p.key, p.value));
            try
            {
                return await request
                    .WithBasicAuth(
                        _authenticationConfig.Basic.UserName, 
                        _authenticationConfig.Basic.Password)
                    .GetJsonAsync<TResult>();
            }
            catch (FlurlHttpException ex)
            {
                var responseString = await ex.Call.Response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<ErrorDetails>(responseString);
                throw new TestApiCallErrorException(error);
            }
        }

        public async Task post_to_sut_api<TRequest>(string path, TRequest request, Action<TRequest> requestModifier = null)
        {
            requestModifier?.Invoke(request);

            var response = await HostAddress
                .AppendPathSegment(path)
                .AllowAnyHttpStatus()
                .WithBasicAuth(
                    _authenticationConfig.Basic.UserName, 
                    _authenticationConfig.Basic.Password)
                .PostJsonAsync(request);

            if (!response.HasOneOfStatuses(HttpStatusCode.OK))
            {
                var error = await response.Content.ToJson<ErrorDetails>();
                Log.Information("Method {path} finished with code: {code} and errorDetails: {@error}", path, response.StatusCode,  error);
                throw new TestApiCallErrorException(error);
            }
        }

        public async Task<TResponse> post_to_sut_api_with_response<TRequest, TResponse>(string path, TRequest request, Action<TRequest> requestModifier = null)
        {
            requestModifier?.Invoke(request);

            var response = await HostAddress
                .AppendPathSegment(path)
                .AllowAnyHttpStatus()
                .WithBasicAuth(
                    _authenticationConfig.Basic.UserName, 
                    _authenticationConfig.Basic.Password)
                .PostJsonAsync(request);
            
            if (!response.HasOneOfStatuses(HttpStatusCode.OK))
            {
                var error = await response.Content.ToJson<ErrorDetails>();
                Log.Information("Method {path} finished with code: {code} and errorDetails: {@error}", path, response.StatusCode,  error);
                throw new TestApiCallErrorException(error);
            }
      
            return await response.Content.ToJson<TResponse>();
        }
        
        public async Task wait_for_projection_to_process_events() => 
            await Host.Services.GetService<MartenProjectionsHost>().WaitForNonStaleResults();

        public virtual void Dispose()
        {
    
        }

    }
}
