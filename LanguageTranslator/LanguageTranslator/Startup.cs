
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace LanguageTranslator
{
    public class Startup
    {
        public IHostingEnvironment HostingEnvironment { get; private set; }
        public IConfiguration Configuration { get; private set; }

        public void Configure(IConfiguration configuration, IHostingEnvironment env)
        {
            this.HostingEnvironment = env;
            this.Configuration = configuration;
        }
    }
}
