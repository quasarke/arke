using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.DSL.Step.Settings;
using Arke.SipEngine.CallObjects;

namespace Arke.SipEngine.Processors
{
    public interface IStepProcessor
    {
        string Name { get; }
        Task DoStep(Step step, ICall call);
    }
}
