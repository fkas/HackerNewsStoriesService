using HackerNewsStoriesApi.Domain;

namespace HackerNewsStoriesApi.Services;

public interface IStoryCache
{ 
    Task<IEnumerable<StorySummary>> GetTopNStoriesAsync(uint count);
}