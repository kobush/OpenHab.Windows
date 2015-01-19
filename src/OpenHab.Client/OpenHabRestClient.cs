using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            var httpClient = new HttpClient(httpClientHandler);
            httpClient.BaseAddress = _baseUri;


            // Add an Accept header for JSON format.
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add an Charset header for Unicode.
            httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

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

        private async Task MakePutRequest(string requestUri, string content, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = GetWebClient())
                {
                    HttpContent httpContent = new StringContent(content, Encoding.UTF8, "text/plain");

                    var response = await client.PutAsync(requestUri, httpContent, cancellationToken);
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
                    HttpContent httpContent = new StringContent(command, Encoding.UTF8, "text/plain");

                    var httpResponse = await client.PostAsync(requestUri, httpContent, cancellationToken);
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

        public async Task<Page> GetPage(Page page, CancellationToken cancellationToken)
        {
            return await MakeGetRequest<Page>(page.Link.PathAndQuery, cancellationToken);
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
