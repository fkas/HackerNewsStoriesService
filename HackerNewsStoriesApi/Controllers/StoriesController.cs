using System.Net;
using HackerNewsStoriesApi.Domain;
using HackerNewsStoriesApi.Exceptions;
using HackerNewsStoriesApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsStoriesApi.Controllers;

[ApiController]
[Route("api/stories")]
public class StoriesController : ControllerBase
{ 
    private readonly ILogger<StoriesController> _logger;
    private readonly IStoryCache _storyCache;

    public StoriesController(ILogger<StoriesController> logger, IStoryCache storyCache)
    {
        _logger = logger;
        _storyCache = storyCache;
    }

    [HttpGet("{count:long}")]
    public async Task<ActionResult<IEnumerable<StorySummary>>> Get(long count)
    {
        try
        {
            return Ok(await _storyCache.GetTopNStoriesAsync((uint)count));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}

