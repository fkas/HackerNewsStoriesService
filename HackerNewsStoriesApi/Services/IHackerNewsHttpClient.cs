namespace HackerNewsStoriesApi.Services;

public interface IHackerNewsHttpClient
{
    Task<HttpResponseMessage> FetchAsync(string url);
}