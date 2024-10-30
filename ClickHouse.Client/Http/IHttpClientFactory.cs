using System.Net.Http;

namespace ClickHouse.Client.Http;

public interface IHttpClientFactory
{
    HttpClient CreateClient(string name);
}
