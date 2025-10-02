namespace Certiminer.Models;

public class VideoProgress
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int VideoId { get; set; }
    public int SecondsCovered { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime? CompletedAt { get; set; }
}
