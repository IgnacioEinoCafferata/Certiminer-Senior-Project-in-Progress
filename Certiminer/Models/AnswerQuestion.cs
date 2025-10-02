﻿namespace Certiminer.Models
{
    public class AnswerQuestion
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
