using System.Net.Http;
using Windows.Security.Cryptography.Certificates;
using Windows.Web.Http.Filters;
using WindowsRuntime.HttpClientFilters;
using OpenHab.Client;

namespace OpenHab.UI.Services
{
    internal class WinRtHttpClientFactory : IHttpClientFactory
    {
        private readonly Settings _settings;

        public WinRtHttpClientFactory(Settings settings)
        {
            _settings = settings;
        }

        HttpClient IHttpClientFactory.Create()
        {
            if (_settings != null && (_settings.IgnoreSslErrors || _settings.DemoMode) )
            {
                var filter = new HttpBaseProtocolFilter();
                filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
                return new System.Net.Http.HttpClient(new WinRtHttpClientHandler(filter));
            }
            else
            {
                return new System.Net.Http.HttpClient();
            }
        }
    }
}