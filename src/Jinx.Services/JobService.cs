using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using Chronos;
using Jinx.Models;
using Jinx.Models.Types;
using Microsoft.CSharp;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Jinx.Services
{
    public class JobService : Service
    {
        [DefaultView("Jobs")]
        public List<Job> Get(GetJobs request)
        {
            return Db.Select<Job>();
        }

        [DefaultView("EditJob")]
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
               return new HttpResult(HttpStatusCode.BadRequest, ex.Message.Replace("\r\n", "")); 
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

            RunJob(job);
            return null;

        }

        private int RunJob(Job job)
        {
            var dbService = ResolveService<DatabaseService>();
            var srcDb = dbService.Get(new GetDatabase {DatabaseId = job.SourceDatabaseId})
                .GetResponseDto<Database>();

            var destDb = dbService.Get(new GetDatabase {DatabaseId = job.DestinationDatabaseId})
                .GetResponseDto<Database>();

            //Get src type

            var compilerService = ResolveService<CompilerService>();
            var compileResults = compilerService.Compile(job.SourceModelCs, job.DestinationModelCs, job.MapCs);


            if (compileResults.Errors.HasErrors)
            {
                throw new Exception("Compile has errors, edit the job to fix the errors");
            }
            var srcType = compileResults.CompiledAssembly.GetType("SourceModel");
            var destType = compileResults.CompiledAssembly.GetType("DestinationModel");
            var transformerType = compileResults.CompiledAssembly.GetType("Transformer");

            var listType = typeof (List<>);

            var srcListType = listType.MakeGenericType(srcType);

            var items = Activator.CreateInstance(srcListType);
            var srcItems = default(List<object>);
            using (var conn = new SqlConnection(srcDb.ConnectionString))
            {
                srcItems = conn.Query(srcType, job.SourceSql).ToList();
            }

            var method = srcListType.GetMethod("Add");
            foreach (var i in srcItems)
            {
                var newItem = Convert.ChangeType(i, srcType);
                method.Invoke(items, new[] {newItem});
            }

            var transformMethod = transformerType.GetMethods(BindingFlags.Static | BindingFlags.Public).First();
            var result = transformMethod.Invoke(null, new[] {items});

            var bcp = new BulkInserter(destDb.ConnectionString, destType);
            bcp.Insert((IEnumerable)result,job.DestinationTableName);

            return 0;
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
            string shell = "using System;\n\npublic class SourceModel\n{\n\t" + properties + "\n}";

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