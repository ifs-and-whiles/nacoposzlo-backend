using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Util;
using Newtonsoft.Json;

namespace Billy.IntegrationTests.Infrastructure.Utils
{
    public static class HttpResponseMessageExtensions
    {
        public static bool HasOneOfStatuses(this HttpResponseMessage response, params HttpStatusCode[] statuses)
        {
            return statuses.Contains(response.StatusCode);
        }
        
        public static async Task<TModel> ToJson<TModel>(this HttpContent content)
        {
            var responseString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TModel>(responseString);
        }

    }
}
