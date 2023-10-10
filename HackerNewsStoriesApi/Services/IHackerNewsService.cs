using HackerNewsStoriesApi.Domain;

namespace HackerNewsStoriesApi.Services;

public interface IHackerNewsService
{
    Task<IEnumerable<long>> FetchTopStoriesAsync();
    Task<StoryDetail> FetchStoryDetailAsync(long storyId);
}