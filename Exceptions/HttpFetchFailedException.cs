namespace HackerNewsStoriesApi.Exceptions;

public class HttpFetchFailedException : Exception
{
    public HttpFetchFailedException(string errorMessage) : base(errorMessage)
    {
    }
}