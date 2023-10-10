namespace HackerNewsStoriesApi.Exceptions;

public class InvalidTopStoriesJsonException : Exception
{
    public InvalidTopStoriesJsonException(string errorMessage, Exception e) : base(errorMessage, e)
    {
    }
}