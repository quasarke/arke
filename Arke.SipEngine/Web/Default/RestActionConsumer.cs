using System;
using System.Threading.Tasks;

namespace Arke.SipEngine.Web.Default
{
    public class RestActionConsumer : IActionConsumer
    {
        private readonly string _endpoint;

        public RestActionConsumer(string endpoint)
        {
            _endpoint = endpoint;
        }

        public IRestCommand GetRestCommand(HttpMethod method, string path)
        {
            return new Command(_endpoint, path)
            {
                UniqueId = Guid.NewGuid().ToString(),
                Method = method.ToString()
            };
        }

        public async Task<IRestCommandResult<T>> ProcessRestCommand<T>(IRestCommand command) where T : new()
        {
            var cmd = (Command) command;
            var result = await cmd.Client.ExecuteTaskAsync<T>(cmd.Request);

            var rtn = new CommandResult<T>
            {
                StatusCode = result.StatusCode,
                Data = result.Data
            };
            return rtn;
        }
        
        public async Task<IRestCommandResult> ProcessRestCommand(IRestCommand command)
        {
            var cmd = (Command) command;
            var result = await cmd.Client.ExecuteTaskAsync(cmd.Request);
            var rtn = new CommandResult()
            {
                StatusCode = result.StatusCode
            };
            return rtn;
        }
    }
}
