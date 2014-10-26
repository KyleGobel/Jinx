using System;
using System.Collections.Generic;
using System.Net;
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

        public HttpResult Get(GetDatabase request)
        {
            if (request == null || request.DatabaseId == default(int))
            {
                return new HttpResult(HttpStatusCode.BadRequest, "Must specify a database id");
            }

            return new HttpResult(Db.Single<Database>(x => x.DatabaseId == request.DatabaseId));
        }
        public HttpResult Post(Database request)
        {
            if (request != null)
            {
                try
                {
                    var id = Db.Insert(request, true);
                    return new HttpResult(HttpStatusCode.Created, "") { Location = "/database/{0}".Fmt(id)};
                }
                catch (Exception e)
                {
                    return new HttpResult(HttpStatusCode.BadRequest, e.Message);
                }
            }
            else 
                return new HttpResult(HttpStatusCode.BadRequest, "Can't insert null database");
        }
    }
}
