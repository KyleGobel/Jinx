using ServiceStack.DataAnnotations;

namespace Jinx.Models.Types
{
    public class Database
    {
        [AutoIncrement]
        public int DatabaseId { get; set; }
        public string Name { get; set; } 
        public string ConnectionString { get; set; }
        public string Type { get; set; }
    }
}