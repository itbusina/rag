using System.ComponentModel.DataAnnotations;
using core.Models;

namespace core.Storage.Models
{
    public class DataSource
    {
        [Key]
        public Guid Id { get; set; }
        public required string Name{ get; set; }
        public required DataSourceType DataSourceType { get; set; }
        public required string DataSourceValue { get; set; }
        public required DateTime CreatedDate { get; set; }
        public required string CollectionName { get; set; }

        // Navigation property
        public ICollection<Assistant> Assistants { get; set; } = [];
    }
}