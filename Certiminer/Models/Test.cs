using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Certiminer.Models
{
    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<Video> Videos { get; set; } = new List<Video>(); // muchos videos pueden apuntar al mismo test

        public int? FolderId { get; set; }
        public Folder? Folder { get; set; }
    }
}

