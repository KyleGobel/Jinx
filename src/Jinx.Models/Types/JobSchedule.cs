using System;
using ServiceStack.DataAnnotations;

namespace Jinx.Models.Types
{
    public class JobSchedule
    {
        public int JobId { get; set; }
        public int Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Active { get; set; }
    }
}