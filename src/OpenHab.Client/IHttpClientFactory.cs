using System.Net.Http;

namespace OpenHab.Client
{
    // See https://github.com/onovotny/WinRtHttpClientHandler
    public interface IHttpClientFactory
    {
        HttpClient Create();
    }
}