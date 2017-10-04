using Stateless;

namespace Arke.SipEngine.FSM
{
    public interface IStateMachine
    {
        StateMachine<State, Trigger> StateMachine { get; set; }
        void Fire(Trigger trigger);
        void SetupFiniteStateMachine();
    }
}