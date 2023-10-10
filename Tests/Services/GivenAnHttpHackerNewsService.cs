using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HackerNewsStoriesApi.Domain;
using HackerNewsStoriesApi.Exceptions;
using HackerNewsStoriesApi.Services;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Tests.Services;

public class GivenAnHttpHackerNewsService
{
    private const long TestStoryId = 1;
    private const string InvalidJson = "Plainly bad json!";
    private readonly IHackerNewsHttpClient _httpClient = Substitute.For<IHackerNewsHttpClient>();
    private HttpHackerNewsService _sut;

    public GivenAnHttpHackerNewsService()
    {
        _sut = new HttpHackerNewsService(_httpClient);
    }
    
    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task WhenFetchTopStoriesFails_ThenAnExceptionThrown(HttpStatusCode responseCode)
    {
        _httpClient.FetchAsync(Arg.Any<string>()).Returns(new HttpResponseMessage(responseCode));

        await Assert.ThrowsAsync<HttpFetchFailedException>(async () => await _sut.FetchTopStoriesAsync());
    }
    
    [Fact]
    public async Task WhenFetchTopStoriesReturnsInvalidJson_ThenExceptionIsThrown()
    {
        _httpClient.FetchAsync(Arg.Any<string>()).Returns(new HttpResponseMessage
            { StatusCode = HttpStatusCode.OK, Content = new StringContent(InvalidJson) });

        await Assert.ThrowsAsync<InvalidTopStoriesJsonException>(async () => await _sut.FetchTopStoriesAsync());
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    public async Task WhenFetchTopStoriesReturnsEmptyJson_ThenEmptyListIsReturned(string topStoriesJson)
    {
        _httpClient.FetchAsync(Arg.Any<string>()).Returns(new HttpResponseMessage
            { StatusCode = HttpStatusCode.OK, Content = new StringContent(topStoriesJson) });

        var result = await _sut.FetchTopStoriesAsync();
        result.ShouldBe(Enumerable.Empty<long>());
    }
    
    [Fact]
    public async Task WhenFetchTopStoriesSucceeds_ThenTopStoryArrayIsReturned()
    {
        var topStories = new[] { 1L, 2L, 3L, 4L };
        
        _httpClient.FetchAsync(Arg.Any<string>()).Returns(new HttpResponseMessage
            { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(topStories)) });

        var result = await _sut.FetchTopStoriesAsync();
        result.ShouldBe(topStories);
    }
    
    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task WhenFetchStoryDetailFails_ThenAnExceptionThrown(HttpStatusCode responseCode)
    {
        _httpClient.FetchAsync(Arg.Any<string>()).Returns(new HttpResponseMessage(responseCode));

        await Assert.ThrowsAsync<HttpFetchFailedException>(async () => await _sut.FetchStoryDetailAsync(TestStoryId));
    }
    
    [Fact]
    public async Task WhenFetchStoryDetailSucceeds_ThenStoryDetailIsReturned()
    {
        var storyDetail = new StoryDetail
        {
            By = "SomeAuthor", 
            Descendants = 10, 
            Score = 9,
            Time = 99999,
            Title = "Hot Story",
            Url = "SomeUrl"
        };

        _httpClient.FetchAsync(Arg.Any<string>()).Returns(new HttpResponseMessage
            { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(storyDetail)) });

        var result = await _sut.FetchStoryDetailAsync(TestStoryId);
        result.ShouldBe(storyDetail);
    }
    
    [Fact]
    public async Task WhenFetchStoryDetailReturnsInvalidJson_ThenExceptionIsThrown()
    {
        _httpClient.FetchAsync(Arg.Any<string>()).Returns(new HttpResponseMessage
            { StatusCode = HttpStatusCode.OK, Content = new StringContent(InvalidJson) });

        await Assert.ThrowsAsync<InvalidStoryDetailJsonException>(async () => await _sut.FetchStoryDetailAsync(TestStoryId));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    public async Task WhenFetchStoryDetailReturnsEmptyJson_ThenExceptionIsThrown(string storyDetailJson)
    {
        _httpClient.FetchAsync(Arg.Any<string>()).Returns(new HttpResponseMessage
            { StatusCode = HttpStatusCode.OK, Content = new StringContent(storyDetailJson) });

        await Assert.ThrowsAsync<StoryDetailMissingException>(async () => await _sut.FetchStoryDetailAsync(TestStoryId));
    }
}