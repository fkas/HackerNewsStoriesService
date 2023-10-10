namespace HackerNewsStoriesApi.Exceptions;

public class StoryDetailMissingException : Exception
{
    public StoryDetailMissingException(string errorMessage) : base(errorMessage)
    {
    }
}