using System.ComponentModel.DataAnnotations;

namespace core.Storage.Models
{
    public class Assistant
    {
        [Key]
        public Guid Id { get; set; }
        public required string Name { get; set; }
        // Navigation property
        public ICollection<DataSource> DataSources { get; set; } = [];
    }
}