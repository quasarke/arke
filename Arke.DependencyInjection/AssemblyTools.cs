using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Arke.DependencyInjection
{
    public class AssemblyTools
    {
        public static IEnumerable<Assembly> GetReferencingAssemblies(string assemblyName)
        {
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            return (from library in dependencies where IsCandidateLibrary(library, assemblyName) select Assembly.Load(new AssemblyName(library.Name))).ToList();
        }

        private static bool IsCandidateLibrary(RuntimeLibrary library, string assemblyName)
        {
            return library.Name == (assemblyName)
                   || library.Dependencies.Any(d => d.Name.StartsWith(assemblyName));
        }
    }
}
