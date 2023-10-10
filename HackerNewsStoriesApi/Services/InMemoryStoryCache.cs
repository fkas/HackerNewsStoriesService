using HackerNewsStoriesApi.Domain;
using HackerNewsStoriesApi.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsStoriesApi.Services;

public class InMemoryStoryCache : IStoryCache
{
    private const string TopStoriesCacheKey = "Top500Stories";
    private const int CacheExpirationInMinutes = 10;
    
    private readonly IMemoryCache _cache;
    private readonly IHackerNewsService _hackerNewsService;
    private static readonly SemaphoreSlim TopNStoriesSemaphore = new SemaphoreSlim(1, 1);

    public InMemoryStoryCache(IMemoryCache cache, IHackerNewsService hackerNewsService)
    {
        _cache = cache;
        _hackerNewsService = hackerNewsService;
    }
    
    public async Task<IEnumerable<StorySummary>> GetTopNStoriesAsync(uint count)
    {
        try 
        {
            await TopNStoriesSemaphore.WaitAsync();
            
            var alreadyCached = _cache.TryGetValue(TopStoriesCacheKey, out List<StorySummary>? topStories);
            if (alreadyCached)
            {
                return topStories == null
                    ? Enumerable.Empty<StorySummary>()
                    : topStories.OrderByDescending(s => s.Score).Take((int)count);
            }

            var topStorySummaries = await FetchTop500StoriesFromHackerNews();
            var storySummaryList = topStorySummaries.ToList();
            CacheStories(storySummaryList);
            return storySummaryList.OrderByDescending(s => s.Score).Take((int)count);

        }
        catch (Exception e)
        {
            throw new InternalSystemException($"Exception caught while fetching top stories: {e.Message}", e);
        } 
        finally 
        {
            TopNStoriesSemaphore.Release();
        }
    }

    private void CacheStories(IEnumerable<StorySummary> topStories)
    {
        var cacheOptions = new MemoryCacheEntryOptions {
            AbsoluteExpiration = DateTime.Now.AddMinutes(CacheExpirationInMinutes),
            Size = 500 * 1024   // Allowing about 1k per story
        };
        
        _cache.Set(TopStoriesCacheKey, topStories.ToList(), cacheOptions);
    }

    private async Task<IEnumerable<StorySummary>> FetchTop500StoriesFromHackerNews()
    {
        var topStoryIds = await _hackerNewsService.FetchTopStoriesAsync();

        var storyIds = topStoryIds.ToList();
        if (!storyIds.Any())
        {
            return Enumerable.Empty<StorySummary>();
        }

        var storySummaryTasks = storyIds.Select(async id => await CreateStorySummary(id));
        return await Task.WhenAll(storySummaryTasks);
    }

    private async Task<StorySummary> CreateStorySummary(long storyId)
    {
        var storyDetail = await _hackerNewsService.FetchStoryDetailAsync(storyId);
            
        return new StorySummary
        {
            PostedBy = storyDetail.By, Title = storyDetail.Title, Score = storyDetail.Score, Uri = storyDetail.Url,
            CommentCount = storyDetail.Descendants, Time = ComputeDateTimeFromUnixTimeStamp(storyDetail.Time)
        };
    }

    private static DateTime ComputeDateTimeFromUnixTimeStamp(ulong unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dateTime.AddSeconds( unixTimeStamp );
    }
}