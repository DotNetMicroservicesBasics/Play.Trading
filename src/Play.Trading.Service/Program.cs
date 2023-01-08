using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Play.Common.Configuration;
using Play.Common.Logging;
using Serilog;

namespace Play.Trading.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
            // .ConfigureLogging(options=>{
            //     options.AddJsonConsole(opts=>{
            //         opts.JsonWriterOptions = new JsonWriterOptions{Indented = true};
            //     });
            // })
            // .ConfigureLogging( (hostingContext, loggingBuilder) =>
            // {
            //     var logger = new LoggerConfiguration()
            //                     .ReadFrom.Configuration(hostingContext.Configuration)
            //                     .CreateLogger();  
            //                    loggingBuilder.AddSerilog(); 
                                       
            // })
            .Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)                
                .AddSeqLogging()
                .ConfigureAzureKeyVault()
                .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    });
    }
}
