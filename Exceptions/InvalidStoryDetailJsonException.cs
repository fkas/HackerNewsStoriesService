namespace HackerNewsStoriesApi.Exceptions;

public class InvalidStoryDetailJsonException : Exception
{
    public InvalidStoryDetailJsonException(string errorMessage, Exception e) : base(errorMessage, e)
    {
    }
}