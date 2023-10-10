namespace HackerNewsStoriesApi.Exceptions;

public class InternalSystemException : Exception
{
    public InternalSystemException(string errorMessage, Exception e) : base(errorMessage, e)
    {
    }
}