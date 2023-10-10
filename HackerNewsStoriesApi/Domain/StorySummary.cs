namespace HackerNewsStoriesApi.Domain;

public record StorySummary
{
    public string? Title { get; set; }
    public string? Uri { get; set; }
    public string? PostedBy { get; set; }
    public DateTime Time { get; set; }
    public required uint Score { get; set; }
    public uint CommentCount { get; set; }
}