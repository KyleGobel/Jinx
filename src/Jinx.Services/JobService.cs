using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using Jinx.Models;
using Jinx.Models.Types;
using ServiceStack;
using ServiceStack.OrmLite;

namespace Jinx.Services
{
    public class JobService : Service
    {
        [DefaultView("Jobs")]
        public List<Job> Get(GetJobs request)
        {
            return Db.Select<Job>();
        }

        public HttpResult Get(GetJob request)
        {
            if (request == null || request.JobId == default(int))
            {
                return new HttpResult(HttpStatusCode.BadRequest, "No job specified");
            }

            return new HttpResult(Db.Single<Job>(x => x.JobId == request.JobId));
        }

        public HttpResult Post(Job job)
        {
            if (job == null)
            {
                return new HttpResult(HttpStatusCode.BadRequest, "Job can't be null");
            }

            try
            {
                var id = Db.Insert(job, true);
                return new HttpResult(HttpStatusCode.Created, "") { Location = "/job/{0}".Fmt(id)};
            }
            catch (Exception ex)
            {
               return new HttpResult(HttpStatusCode.BadRequest, ex.Message); 
            }
        }

        public object Get(RunJob runJobRequest)
        {
            if (runJobRequest == null || runJobRequest.JobId == default(int))
            {
                return new HttpResult(HttpStatusCode.BadRequest, "No job specified");
            }

            var job = Get(runJobRequest.ConvertTo<GetJob>()).GetResponseDto<Job>();

            if (job == null)
            {
                return HttpError.NotFound("Couldn't find job with that job id");
            }

            var dbService = ResolveService<DatabaseService>();
            var database = dbService.Get(new GetDatabase {DatabaseId = job.SourceDatabaseId})
                .GetResponseDto<Database>();
            var typeNameDictionary = BuildSrcModelDictionary(database.ConnectionString, database.Type, job.SourceSql);
            var model = BuildModel(typeNameDictionary);
            return null;

        }

        public HttpResult Post(BuildModel request)
        {
            if (request == null || request.DatabaseId == default(int) || request.Sql.IsNullOrEmpty())
            {
               return new HttpResult(HttpStatusCode.BadRequest, "Invalid request"); 
            }
            var dbService = ResolveService<DatabaseService>();

            var database = dbService.Get(new GetDatabase {DatabaseId = request.DatabaseId}).GetResponseDto<Database>();
            var dict = BuildSrcModelDictionary(database.ConnectionString, database.Type, request.Sql);

            var model = BuildModel(dict);

            return new HttpResult(model);
        }
        private string BuildModel(Dictionary<string, Type> typeDictionary)
        {
            var props = typeDictionary.Select(x => "public " + x.Value.Name + " " + x.Key + " {get; set;}");
            var properties = props.Aggregate((a, b) => a + "\n\t" + b);
            string shell = "public class Model\n{\n\t" + properties + "\n}";

            return shell;
        }
        private Dictionary<string, Type> BuildSrcModelDictionary(string connectionString, string dbType, string sql)
        {
            if (dbType != "MsSql")
                return null;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();

                    var typeNameDictionary = new Dictionary<string, Type>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        typeNameDictionary.Add(reader.GetName(i), reader.GetFieldType(i));
                    }
                    return typeNameDictionary;
                }
            }


        }

        private void RunJob(Database srcDatabase, Job job)
        {
            if (srcDatabase.Type != "MsSql")
                return;

            using (var connection = new SqlConnection(srcDatabase.ConnectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(job.SourceSql, connection))
                {
                    var reader = cmd.ExecuteReader();

                    var typeNameDictionary = new Dictionary<string, Type>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        typeNameDictionary.Add(reader.GetName(i), reader.GetFieldType(i));
                    }

                    dynamic dataReader = new DynamicDataReader(cmd.ExecuteReader());

                    while (dataReader.Read())
                    {
                        
                    }
                }
            }
        }
    }
}