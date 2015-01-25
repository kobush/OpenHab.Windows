using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

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
            var filter = new HttpBaseProtocolFilter();
            //filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted); - not needed when cert is installed
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.InvalidName);

            var httpClient = new HttpClient(filter);

            // Add an Accept header for JSON format.
            httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));

            // Add an Charset header for Unicode.
            httpClient.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");

            return httpClient;
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
            var query = GetDefaultQueryString();

            if (query.Any())
            {
                if (!requestUri.EndsWith("?"))
                    requestUri += "?";
                requestUri = requestUri + FormatAsQueryString(query);
            } 

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseUri, requestUri));

            try
            {
                using (var client = GetWebClient())
                {

                    var response = await client.SendRequestAsync(request).AsTask(cancellationToken);
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

                    var json = await response.Content.ReadAsStringAsync().AsTask(cancellationToken);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                if ((ex.HResult & 65535) == 12045)
                {
                    // Get a list of the server cert errors
                    IReadOnlyList<ChainValidationResult> errors = request.TransportInformation.ServerCertificateErrors;
                }

                throw new Exception("Request failed", ex);
            }
        }

        private async Task MakePutRequest(string requestUri, string content, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = GetWebClient())
                {
                    IHttpContent httpContent = new HttpStringContent(content, UnicodeEncoding.Utf8, "text/plain");

                    var response = await client.PutAsync(new Uri(_baseUri, requestUri), httpContent).AsTask(cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("PUT request failed with status code " + response.StatusCode);
                    }

                    //return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PUT request failed", ex);
            }
        }

        private async Task MakePostRequest(string requestUri, string command, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = GetWebClient())
                {
                    IHttpContent httpContent = new HttpStringContent(command, UnicodeEncoding.Utf8, "text/plain");

                    var httpResponse = await client.PostAsync(new Uri(_baseUri,  requestUri), httpContent).AsTask(cancellationToken);
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        var msg = string.Format("POST request failed with status code {0}", httpResponse.StatusCode);
                        var responseString = await httpResponse.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseString))
                            msg += "\n" + responseString;

                        throw new Exception(msg);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("POST request failed", ex);
            }
        }

        public async Task<IEnumerable<Item>> GetItems(CancellationToken cancellationToken)
        {
            var list = await MakeGetRequest<ItemsList>("/rest/items", cancellationToken);
            return list != null ? list.Item : Enumerable.Empty<Item>();
        }

        public async Task<Item> GetItem(Item item, CancellationToken cancellationToken)
        {
            //TODO: use link ???
            return (await MakeGetRequest<Item>("/rest/items/" + item.Name, cancellationToken));
        }

        public async Task<IEnumerable<Sitemap>> GetSitemaps(CancellationToken cancellationToken)
        {
            var list = await MakeGetRequest<SitemapList>("/rest/sitemaps", cancellationToken);
            return list != null ? list.Sitemap : Enumerable.Empty<Sitemap>();
        }

        public async Task<SitemapPage> GetPage(Page page, CancellationToken cancellationToken)
        {
            return await MakeGetRequest<SitemapPage>(page.Link.PathAndQuery, cancellationToken);
        }

        public async Task SetItemState(Item item, string newState, CancellationToken cancellationToken)
        {
            await MakePutRequest(item.Link.PathAndQuery + "/state", newState, cancellationToken);
        }

        public async Task SendItemCommand(Item item, string command, CancellationToken cancellationToken)
        {
            await MakePostRequest(item.Link.PathAndQuery , command, cancellationToken);
        }
    }

    internal class ItemsList
    {
        [JsonConverter(typeof(SingleOrArrayConverter<Item>))]
        public IList<Item> Item { get; set; }
    }

    internal class SitemapList
    {
        [JsonConverter(typeof(SingleOrArrayConverter<Sitemap>))]
        public IList<Sitemap> Sitemap { get; set; }
    }

    class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }
            return new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
