using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace OpenHab.Client
{
    public class OpenHabRestClient
    {
        private readonly Uri _baseUri;

        /// <summary>
        /// Creates OpenHabRestClient instance.
        /// </summary>
        /// <param name="baseUri">Base url of openHAB server eg. http://localhost:8080</param>
        public OpenHabRestClient(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        private HttpClient GetWebClient()
        {
            var httpClientHandler = new HttpClientHandler();

            var client = new HttpClient(httpClientHandler);

            client.BaseAddress = _baseUri;
            

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private IDictionary<string, string> GetDefaultQueryString()
        {
            var query = new Dictionary<string, string>();
            return query;
        }

        private string FormatAsQueryString(IDictionary<string, string> query)
        {
            var sb = new StringBuilder();
            foreach (var pair in query)
            {
                if (sb.Length > 0)
                    sb.Append("&");

                sb.Append(WebUtility.UrlEncode(pair.Key));
                sb.Append("=");
                sb.Append(WebUtility.UrlEncode(pair.Value));
            }
            return sb.ToString();
        }

        private async Task<T> MakeGetRequest<T>(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = GetWebClient())
                {
                    var query = GetDefaultQueryString();

                    /*if (appendQuery != null)
                        appendQuery(query);*/

                    if (query.Any())
                    {
                        if (!requestUri.EndsWith("?"))
                            requestUri += "?";
                        requestUri = requestUri + FormatAsQueryString(query);
                    }

                    // make request
                    var response = await client.GetAsync(requestUri, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        try
                        {
                            // try to parse error message from response body
                           /* var errorResponse = await response.Content.ReadAsAsync<ErrorResponse>(cancellationToken);
                            if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Error))
                                throw new Exception(errorResponse.Error);*/
                        }
                        catch (Exception)
                        { }

                        throw new Exception("Request failed with status code " + response.StatusCode);
                    }

                    return await response.Content.ReadAsAsync<T>(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Request failed", ex);
            }
        }

        public async Task<IEnumerable<ItemSummary>> GetItems(CancellationToken cancellationToken)
        {
            var list = await MakeGetRequest<ItemsList>("/rest/items", cancellationToken);
            return list != null ? list.Item : Enumerable.Empty<ItemSummary>();
        }

        public async Task<ItemSummary> GetItem(ItemSummary itemSummary, CancellationToken cancellationToken)
        {
            //TODO: use link
            return (await MakeGetRequest<ItemSummary>("/rest/items/" + itemSummary.Name, cancellationToken));
        }
    }

    internal class ItemsList
    {
        public IList<ItemSummary> Item { get; set; }
    }
}
