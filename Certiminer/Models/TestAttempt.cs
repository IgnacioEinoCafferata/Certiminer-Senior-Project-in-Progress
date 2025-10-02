using System;

namespace Certiminer.Models
{
    public class TestAttempt
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int TestId { get; set; }
        public Test Test { get; set; } = null!;

        public int Score { get; set; }
        public int Total { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
