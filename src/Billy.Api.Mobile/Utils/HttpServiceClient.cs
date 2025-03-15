using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Billy.Api.Mobile.Infrastructure.Configs;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Serilog;

namespace Billy.Api.Mobile.Utils
{
    public class HttpServiceClient
    {
        private readonly IFlurlClient _flurlClient;
        private readonly ILogger _logger = Log.ForContext<HttpServiceClient>();
        private readonly AsyncRetryPolicy _retryPolicy;

        public HttpServiceClient(
            string serviceBaseUrl,
            IFlurlClientFactory flurlClientFactory,
            RequestRetryPolicyConfig requestRetryPolicyConfig)
        {
            // The author's recommended creation method. Flurl Client uses http client under the hood.
            // HttpClient is intended to be instantiated once and re-used throughout the life of an application.
            // Especially in server applications, creating a new HttpClient instance for every request will exhaust the number of sockets available under heavy loads.
            // This will result in SocketException errors.
            // Flurl caches httpclient per endpoint and takes care of correct disposing.
            _flurlClient = flurlClientFactory.Get(serviceBaseUrl);

            _retryPolicy = Policy
                .Handle<FlurlHttpException>(WhenHttpStatusDifferentThatBadRequestFamily())
                .WaitAndRetryAsync(
                    requestRetryPolicyConfig.RetryCount, 
                    sleepDuration =>  TimeSpan.FromMilliseconds(requestRetryPolicyConfig.MilisecondsBetweenRetries),
                    onRetry: (exception, retryCount, context) =>
                {
                    _logger.Warning($"Retrying {retryCount} time on service http error...");
                });
        }

        //TODO: [FP] reimplement - temp solution
        private static Func<FlurlHttpException, bool> WhenHttpStatusDifferentThatBadRequestFamily() =>
            x => !((int)x.Call.HttpStatus).ToString().StartsWith("4");


        public async Task<TResult> Get<TResult>(
            string methodUrl,
            Guid correlationId,
            params (string key, object value)[] queryParams)
        {
            var flurlRequest = _flurlClient.Request(methodUrl);
            flurlRequest.Headers.Add("X-Correlation-ID", correlationId);
            queryParams.ForEach(p => flurlRequest.SetQueryParam(p.key, p.value));

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    return await flurlRequest.GetJsonAsync<TResult>();
                }
                catch (FlurlHttpException ex)
                {
                    _logger.Error(ex, "Error while calling Get http method for {methodUrl} with {@queryParams}", methodUrl, queryParams);
                    throw;
                }
            });
        }

        public async Task Post<TRequest>(
            string methodUrl,
            Guid correlationId,
            TRequest request)
        {
            var flurlRequest = _flurlClient.Request(methodUrl);
            flurlRequest.Headers.Add("X-Correlation-ID", correlationId);
            await _retryPolicy.ExecuteAsync(async () => {
               
                try
                {
                    await flurlRequest
                        .AllowHttpStatus(HttpStatusCode.Accepted, HttpStatusCode.OK)
                        .PostJsonAsync(request);
                }
                catch (FlurlHttpException ex)
                {
                    _logger.Error(ex, "Error while calling Post http method for {methodUrl} with {@request}", methodUrl, request);
                    throw;
                }
            });
        }

    }

    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items) action(item);
        }
    }
}
