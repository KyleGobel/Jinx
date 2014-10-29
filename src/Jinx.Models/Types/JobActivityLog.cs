using System;
using ServiceStack.DataAnnotations;

namespace Jinx.Models.Types
{
    public class JobActivityLog
    {
        [PrimaryKey, AutoIncrement]
        public int JobActivityLogId { get; set; } 
        public int JobId { get; set; }
        public DateTime StartTime { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }
}