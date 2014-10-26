using System.Collections.Generic;
using Jinx.Models.Types;
using ServiceStack;

namespace Jinx.Models
{
    public class GetDatabases : IReturn<List<Database>> { }

    public class GetDatabase
    {
        public int DatabaseId { get; set; }
    }
}