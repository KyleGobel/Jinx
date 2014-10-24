using System;
using System.Collections.Generic;
using Jinx.Models;
using Jinx.Models.Types;
using ServiceStack;
using ServiceStack.OrmLite;

namespace Jinx.Services
{
    public class DatabaseService : Service
    {
        [DefaultView("Database")]
        public List<Database> Get(GetDatabases request)
        {
            var results = Db.Select<Database>();

            return results;
        }

    }
}
