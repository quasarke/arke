using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using SimpleInjector;

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
