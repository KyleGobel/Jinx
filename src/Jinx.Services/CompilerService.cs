using System;
using System.CodeDom.Compiler;
using System.Net;
using Jinx.Models;
using Microsoft.CSharp;
using ServiceStack;
using ServiceStack.Text;

namespace Jinx.Services
{
    public class CompilerService : Service
    {
        public HttpResult Post(CompileAll request)
        {
            var extraLines = 0;
            var output = "";

            request.SourceModel = AddNamespaceToSrc(request.SourceModel);
            request.DestinationModel = AddNamespaceToSrc(request.DestinationModel);
            //var assemblyFile = "gen" + Guid.NewGuid().ToString("N") + ".dll";

            var results = Compile(request.SourceModel, request.DestinationModel, request.Map);

            if (results.Errors.HasErrors)
            {
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    output += results.Errors[i].ErrorText + " :: Line " + (results.Errors[i].Line + extraLines);
                    output += "\r\n";
                }
            }
            else
            {
                output = "Compile Success";
            }
            return new HttpResult(output); 
        }

        public CompilerResults Compile(params string[] sources)
        {
             var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            return provider.CompileAssemblyFromSource(parameters, sources);
        }

        private string AddNamespaceToSrc(string src)
        {
            return "using System;\n" + src;
        }
        public HttpResult Post(Compile request)
        {
            var src = "";
            var extraLines = 0;
            var output = "";
            if (request.Type == "map")
            {
                return null;
            }

            if (request.Type == "sourceModel")
            {
                src = AddNamespaceToSrc(request.Source);
                extraLines = 1;
            }
            //var assemblyFile = "gen" + Guid.NewGuid().ToString("N") + ".dll";

            var provider = new CSharpCodeProvider();
            var parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };

            var results = provider.CompileAssemblyFromSource(parameters, src);

            if (results.Errors.HasErrors)
            {
                for(int i =0; i<results.Errors.Count; i++)
                {
                    output += results.Errors[i].ErrorText + " :: Line " + (results.Errors[i].Line + extraLines);
                    output += "\r\n";
                }
            }
            else
            {
                output = "Compile Success";
            }
            return new HttpResult(output);
        }
    }
}