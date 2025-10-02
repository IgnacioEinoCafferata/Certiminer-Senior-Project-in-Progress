namespace Certiminer.Repositories
{
    public sealed class ChapterCount
    {
        public Certiminer.Models.Test Test { get; init; } = default!;
        public int Count { get; init; }
    }
}
