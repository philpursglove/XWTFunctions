using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(XWTFunctions.Startup))]

namespace XWTFunctions
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            //builder.Services.AddSingleton<IMyService>((s) => { return new MyService(); });

            //builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }
    }
}

