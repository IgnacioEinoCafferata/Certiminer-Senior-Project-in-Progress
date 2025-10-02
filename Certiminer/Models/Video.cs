using System.ComponentModel.DataAnnotations;

namespace Certiminer.Models
{
    public class Video
    {
        public int Id { get; set; }

        [Required, StringLength(256)]
        public string Title { get; set; } = string.Empty;

        // Acepta URL o ID normalizado (guardás el ID de YouTube después)
        [Required, StringLength(2048)]
        public string Url { get; set; } = string.Empty;

        // 1 = YouTube, 2 = File (o lo que uses)
        public int SourceType { get; set; }
        public bool IsActive { get; set; } = true;

        public int? TestId { get; set; }
        public Test? Test { get; set; }

        // Carpeta (opcional)
        public int? FolderId { get; set; }
        public Folder? Folder { get; set; }
    }
}
