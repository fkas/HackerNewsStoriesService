using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackerNewsStoriesApi.Domain;
using HackerNewsStoriesApi.Services;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Tests.Services;

public class GivenInMemoryStoryCache
{
    private readonly IMemoryCache _cache = Substitute.For<IMemoryCache>();
    private readonly IHackerNewsService _hackerNewsService = Substitute.For<IHackerNewsService>();
    private InMemoryStoryCache _sut;
    private const uint StoryCount = 5;

    private static readonly long[] TopStoryIds = { 1L, 2L, 3L, 4L, 5L };
    private static readonly StorySummary[] TopStories =
    {
        new() {PostedBy = "Author1", Score = 5, Title = "Story1", Time = DateTime.Today.AddDays(-5), Uri = "Url1", CommentCount = 5},
        new() {PostedBy = "Author2", Score = 4, Title = "Story2", Time = DateTime.Today.AddDays(-4), Uri = "Url1", CommentCount = 4},
        new() {PostedBy = "Author3", Score = 3, Title = "Story3", Time = DateTime.Today.AddDays(-3), Uri = "Url1", CommentCount = 3},
        new() {PostedBy = "Author4", Score = 2, Title = "Story4", Time = DateTime.Today.AddDays(-2), Uri = "Url1", CommentCount = 2},
        new() {PostedBy = "Author5", Score = 1, Title = "Story5", Time = DateTime.Today.AddDays(-1), Uri = "Url1", CommentCount = 1}
    };

    public GivenInMemoryStoryCache()
    {
        _sut = new InMemoryStoryCache(_cache, _hackerNewsService);
    }

    [Fact]
    public async Task WhenGetTopNStoriesIsInvokedForTheFirstTime_ThenTopStoriesAndDetailsAreFetchedFromHackerNewsAndCached()
    {
        MakeHackerNewsServiceReturnTopStories();

        _cache.TryGetValue(Arg.Any<object>(), out Arg.Any<object?>()).Returns(false);
        
        var result = await _sut.GetTopNStoriesAsync(3);

        await _hackerNewsService.Received(1).FetchTopStoriesAsync();

        await _hackerNewsService.Received(1).FetchStoryDetailAsync(1);
        await _hackerNewsService.Received(1).FetchStoryDetailAsync(2);
        await _hackerNewsService.Received(1).FetchStoryDetailAsync(3);
        await _hackerNewsService.Received(1).FetchStoryDetailAsync(4);
        await _hackerNewsService.Received(1).FetchStoryDetailAsync(5);

        _cache.Received(1).CreateEntry(Arg.Is<object>(k => k != null));
        
        // Only topN stories are returned in descending order of score
        var resultStoryList = result.ToList();
        resultStoryList.ShouldBeEquivalentTo(new [] {TopStories[0], TopStories[1], TopStories[2]}.ToList());
        resultStoryList.ShouldBeInOrder( SortDirection.Descending, new StorySummaryComparer());
    }

    [Fact]
    public async Task WhenGetTopNStoriesIsInvokedForTheFirstTimeConcurrently_ThenTopStoriesAreReadAndCachedOnlyOnce()
    {
        MakeHackerNewsServiceReturnTopStories();

        _cache.TryGetValue(Arg.Any<object>(), out Arg.Any<object?>()).Returns(_ => false, _ => true, _ => true);

        var result = InvokeGetTopNStoriesInParallel();

        await _hackerNewsService.Received(1).FetchTopStoriesAsync();

        await _hackerNewsService.Received(1).FetchStoryDetailAsync(1);
        await _hackerNewsService.Received(1).FetchStoryDetailAsync(2);
        await _hackerNewsService.Received(1).FetchStoryDetailAsync(3);
        await _hackerNewsService.Received(1).FetchStoryDetailAsync(4);
        await _hackerNewsService.Received(1).FetchStoryDetailAsync(5);

        _cache.Received(1).CreateEntry(Arg.Is<object>(k => k != null));
        
        result.Count().ShouldBe(3);
    }

    [Fact]
    public async Task WhenGetTopNStoriesIsInvokedASubsequentTime_ThenTopStoriesAreReadFromCache()
    {
        MakeHackerNewsServiceReturnTopStories();

        _cache.TryGetValue(Arg.Any<object>(), out Arg.Any<object?>()).Returns(true);

        await _sut.GetTopNStoriesAsync(StoryCount);

        await _hackerNewsService.Received(0).FetchTopStoriesAsync();

        await _hackerNewsService.Received(0).FetchStoryDetailAsync(Arg.Any<long>());

        _cache.Received(0).CreateEntry(Arg.Is<object>(k => k != null));
    }
    
    private IEnumerable<IEnumerable<StorySummary>> InvokeGetTopNStoriesInParallel()
    {
        var tasks = new List<Task<IEnumerable<StorySummary>>>();

        Parallel.Invoke(
             () =>
            { 
                tasks.Add(_sut.GetTopNStoriesAsync(StoryCount));
            },
             () =>
            {
                tasks.Add (_sut.GetTopNStoriesAsync(StoryCount));
            }, 
             () =>
            {
                tasks.Add(_sut.GetTopNStoriesAsync(StoryCount));
            });

        return Task.WhenAll(tasks).Result;
    }

    private void MakeHackerNewsServiceReturnTopStories()
    {
        _hackerNewsService.FetchTopStoriesAsync().Returns(TopStoryIds);

        foreach (var storyId in TopStoryIds)
        {
            _hackerNewsService.FetchStoryDetailAsync(Arg.Is(storyId)).Returns(new StoryDetail
            {
                By = TopStories[storyId - 1].PostedBy, 
                Descendants = TopStories[storyId - 1].CommentCount,
                Score = TopStories[storyId - 1].Score, 
                Title = TopStories[storyId - 1].Title,
                Time = ConvertToUnixTime(TopStories[storyId - 1].Time),
                Url = TopStories[storyId - 1].Uri
            });
        }
    }
    
    private static ulong ConvertToUnixTime(DateTime time)
    {
        return (ulong)time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private class StorySummaryComparer : IComparer<StorySummary>
    {
        public int Compare(StorySummary? x, StorySummary? y)
        {
            return x.Score < y.Score ? -1 : x.Score == y.Score ? 0 : 1;
        }
    }
}