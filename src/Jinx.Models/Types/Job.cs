using System;
using ServiceStack.DataAnnotations;

namespace Jinx.Models.Types
{
    public class Job
    {
        [AutoIncrement]
        public int JobId { get; set; }
        public string Name { get; set; } 
        public long Interval { get; set; }
        public string SourceSql { get; set; }
        public int SourceDatabaseId { get; set; }
        public int DestinationDatabaseId { get; set; }
        public string SourceModelCs { get; set; }
        public string DestinationModelCs { get; set; }
        public string DestinationTableName { get; set; }
        public string MapCs { get; set; }
    }
}