namespace HackerNewsStoriesApi.Services;

public class HackerNewsHttpClient : IHackerNewsHttpClient
{
    private readonly HttpClient _httpClient = new HttpClient();
    
    public async Task<HttpResponseMessage> FetchAsync(string url)
    {
        return await _httpClient.GetAsync(url).ConfigureAwait(false);
    }
}