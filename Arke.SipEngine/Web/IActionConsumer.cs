using System.Threading.Tasks;

namespace Arke.SipEngine.Web
{
    public interface IActionConsumer
    {
        IRestCommand GetRestCommand(HttpMethod method, string path);
        Task<IRestCommandResult<T>> ProcessRestCommand<T>(IRestCommand command) where T : new();
        Task<IRestCommandResult> ProcessRestCommand(IRestCommand command);
    }
}
