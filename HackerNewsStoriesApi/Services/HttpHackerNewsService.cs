using System.Text.Json;
using HackerNewsStoriesApi.Domain;
using HackerNewsStoriesApi.Exceptions;

namespace HackerNewsStoriesApi.Services;

public class HttpHackerNewsService : IHackerNewsService
{
    private const string TopStoriesUrl = "https://hacker-news.firebaseio.com/v0/beststories.json";
    private const string StoryDetailUrl = "https://hacker-news.firebaseio.com/v0/item/{0}.json";
    
    private readonly IHackerNewsHttpClient _httpClient;

    public HttpHackerNewsService(IHackerNewsHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IEnumerable<long>> FetchTopStoriesAsync()
    {
        try
        {
            var result = await _httpClient.FetchAsync(TopStoriesUrl);

            if (!result.IsSuccessStatusCode)
            {
                throw new HttpFetchFailedException($"Fetch top stories failed with status code {result.StatusCode}");
            }

            var topStoriesText = await result.Content.ReadAsStringAsync();
            
            if (string.IsNullOrEmpty(topStoriesText) || topStoriesText == "{}")
            {
                return Enumerable.Empty<long>();
            }
            
            var topStories = JsonSerializer.Deserialize<long[]>(topStoriesText);

            return topStories ?? Enumerable.Empty<long>();
        }
        catch (JsonException e)
        {
            throw new InvalidTopStoriesJsonException(
                $"Failed to retrieve top stories. Bad Json [{e.Message}]", e);            
        }
    }

    public async Task<StoryDetail> FetchStoryDetailAsync(long storyId)
    {
        try
        { 
            var result = await _httpClient.FetchAsync(string.Format(StoryDetailUrl, storyId));

            if (!result.IsSuccessStatusCode)
            {
                throw new HttpFetchFailedException($"Fetch story detail for story-id [{storyId}] failed with status code {result.StatusCode}");
            }
        
            var storyDetailText = await result.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(storyDetailText) || storyDetailText == "{}")
            {
                throw new StoryDetailMissingException($"No details found at Hacker News for story-id [{storyId}]");
            }

            var storyDetail = JsonSerializer.Deserialize<StoryDetail>(storyDetailText);

            if (storyDetail == null)
            {
                throw new StoryDetailMissingException($"No details found at Hacker News for story-id [{storyId}]");
            }

            return storyDetail;
        }
        catch (JsonException e)
        {
            throw new InvalidStoryDetailJsonException(
                $"Failed to retrieve story detail for story-id [{storyId}]. Bad Json [{e.Message}]", e);
        }
    }
}