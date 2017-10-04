using System.Threading.Tasks;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;

namespace Arke.SipEngine.Processors
{
    public interface IStepProcessor
    {
        string Name { get; }
        Task DoStep(ISettings settings, ICall call);
    }
}
