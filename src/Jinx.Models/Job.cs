using System.Collections.Generic;
using Jinx.Models.Types;
using ServiceStack;

namespace Jinx.Models
{
    public class GetJobs : IReturn<List<Job>>  { }

    public class RunJob
    {
        public int JobId { get; set; }
    }

    public class GetJob
    {
        public int JobId { get; set; }
    }

    public class BuildModel
    {
        public int DatabaseId { get; set; }
        public string Sql { get; set; }
    }
}