using Microsoft.Extensions.Configuration;
using Serilog;

namespace Arke.DependencyInjection
{
    public static class ContainerExtensions
    {
        public static void RegisterLogging(this ObjectContainer container, IConfiguration configuration)
        {
            container.Register<ILogger>(() => new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger());
        }
    }
}
