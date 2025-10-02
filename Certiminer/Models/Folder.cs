using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Certiminer.Models
{
    public class Folder
    {
        public int Id { get; set; }

        [Required, StringLength(128)]
        public string Name { get; set; } = string.Empty;

        public FolderKind Kind { get; set; }

        public int? ParentId { get; set; }
        public Folder? Parent { get; set; }
        public ICollection<Folder> Children { get; set; } = new List<Folder>();
    }
}
