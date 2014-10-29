using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Funq;
using Jinx.Models;
using Jinx.Models.Types;
using Jinx.Resources;
using Jinx.Services;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Razor;
using ServiceStack.Text;

namespace Jinx
{
    public class AppHost : AppHostHttpListenerBase
    {
        public const string JinxConnectionString = "Data Source=jinx.db;Version=3;";
        public const string BaseUrl = "http://localhost:33936/";
        public AppHost() : base("Jinx", typeof(DatabaseService).Assembly) { }

        public override void Configure(Container container)
        {
            JsConfig.EmitCamelCaseNames = true;
            container.Register<IDbConnectionFactory>(c => new OrmLiteConnectionFactory(JinxConnectionString, SqliteDialect.Provider));
            SetConfig(new HostConfig()
            {
                EmbeddedResourceBaseTypes = { GetType(), typeof(BaseTypeMarker)}
            });
            Plugins.Add(new RazorFormat
            {
                LoadFromAssemblies = { typeof(BaseTypeMarker).Assembly}
            });

            using (var db = container.Resolve<IDbConnectionFactory>().Open())
            {
                db.CreateTableIfNotExists<Database>();
                db.CreateTableIfNotExists<Job>();
                db.CreateTableIfNotExists<JobActivityLog>();
                db.CreateTableIfNotExists<JobSchedule>();
            }
            AddRoutes();
        }

        public void AddRoutes()
        {
            Routes.Add<GetDatabases>("/database", "GET");
            Routes.Add<Database>("/database", "POST");
            Routes.Add<GetJobs>("/job", "GET")
                .Add<Job>("/job", "POST, PUT")
                .Add<GetJob>("/job/{JobId}", "GET");
            Routes
                .Add<RunJob>("/runJob/{JobId}", "GET")
                .Add<BuildModel>("/job/buildModel", "POST");

            Routes
                .Add<Compile>("/compile", "POST")
                .Add<CompileAll>("/compileAll", "POST");

        }
    }
    class Program
    {
       
        static void Main(string[] args)
        {

            var appHost = new AppHost()
                .Init()
                .Start(AppHost.BaseUrl);

            JobEngine.Start(appHost.Container);
            Console.WriteLine("Jinx started at {0}, listening on port {1}",
                DateTime.Now, 33936);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public static class JobEngine
    {

        private static Container _container;
        public static void Start(Funq.Container container)
        {
            _container = container;
            Task.Factory.StartNew(ProcessLoop);
        }

        public static void ProcessLoop()
        {
            for (;;)
            {
                var jobs = FindJobsToRun();

                var jobRequests = jobs.ToDictionary(x => x.JobId, x => AppHost.BaseUrl + (new RunJob {JobId = x.JobId}).ToGetUrl());


                foreach (var request in jobRequests)
                {
                    SetJobStatusToRunning(request.Key);
                    var result = request.Value.GetJsonFromUrl();
                    //do stuff depending on what result we get
                }

                Thread.Sleep(TimeSpan.FromMinutes(2));
            }
        }

        private static void SetJobStatusToRunning(int jobId)
        {
            const int runningStatus = 1;
            using (var db = _container.Resolve<IDbConnectionFactory>().Open())
            {
                db.ExecuteNonQuery("update jobschedule set status = @status, starttime = @startTime where jobid = @jobId", new
                {
                   status = runningStatus,
                   startTime = DateTime.UtcNow,
                   jobId = jobId
                });
            }
        }

        public static List<Job> FindJobsToRun()
        {
            try
            {
                using (var db = _container.Resolve<IDbConnectionFactory>().Open())
                {
                    var jobIntervals = db.Dictionary<int, long>("select job.jobid, interval from job inner join jobschedule on jobschedule.jobid = job.jobid where jobschedule.active = '1'");

                    var jobLastStarts = db.Dictionary<int, DateTime?>(
                        "select s.jobid, s.endtime from JobSchedule s where s.status = 0 and s.active = '1' ");

                    var jobIds = jobLastStarts.Where(x =>
                    {
                        if (x.Value == null)
                            return true;
                        var lastRun = x.Value.Value;

                        if (jobIntervals.ContainsKey(x.Key) &&
                            (lastRun + jobIntervals[x.Key].ToTimespan()) > DateTime.UtcNow)
                        {
                            return true;
                        }
                        return false;
                    }).Select(x => x.Key);

                    return db.SelectByIds<Job>(jobIds);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in JobEngine {0}", e);

                throw;
            }
        }

    }
}
