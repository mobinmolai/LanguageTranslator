using LanguageTranslator.Helper.Multilingual;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;

namespace LanguageTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildWebHost(args).Run();

            Employee employee = new Employee() { Id = 1, Name = "Khan" };
       
          //  LanguageTranslator l = new 

        }
        public static IWebHost BuildWebHost(string[] args)
        {
            var s = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

             return s.Build();

        }
    } 
}
