using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
        public AppHost() : base("Jinx", typeof(DatabaseService).Assembly) { }

        public override void Configure(Container container)
        {
            JsConfig.EmitCamelCaseNames = true;
            container.Register<IDbConnectionFactory>(c => new OrmLiteConnectionFactory("Data Source=jinx.db;Version=3;", SqliteDialect.Provider));
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
            }
            AddRoutes();
        }

        public void AddRoutes()
        {
            Routes.Add<GetDatabases>("/database", "GET");
            Routes.Add<Database>("/database", "POST");
            Routes.Add<GetJobs>("/job", "GET");
            Routes.Add<Job>("/job", "POST");
            Routes.Add<RunJob>("/runJob/{JobId}", "GET")
                .Add<BuildModel>("/job/buildModel", "POST");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var appHost = new AppHost()
                .Init()
                .Start("http://*:33936/");

            Console.WriteLine("Jinx started at {0}, listening on port {1}",
                DateTime.Now, 33936);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
