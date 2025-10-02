using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Certiminer.Models
{
    public enum QuestionType { SingleChoice = 0 }

    public class Question
    {
        public int Id { get; set; }

        public int TestId { get; set; }     // <<< ya no VideoId
        public Test? Test { get; set; }

        public string Prompt { get; set; } = string.Empty;
        public QuestionType Type { get; set; } = QuestionType.SingleChoice;
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<AnswerQuestion> Options { get; set; } = new List<AnswerQuestion>();
    }
}

