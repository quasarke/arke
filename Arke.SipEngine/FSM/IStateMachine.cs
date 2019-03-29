using System.Threading.Tasks;
using Stateless;

namespace Arke.SipEngine.FSM
{
    public interface IStateMachine
    {
        StateMachine<State, Trigger> StateMachine { get; set; }
        Task FireAsync(Trigger trigger);
        void SetupFiniteStateMachine();
    }
}