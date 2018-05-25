using Arke.SipEngine.CallObjects;
using Stateless;

namespace Arke.SipEngine.FSM
{
    public class CallStateMachine : IStateMachine
    {
        private readonly ICall _call;
        private readonly IPromptPlayer _promptPlayer;
        public StateMachine<State, Trigger> StateMachine { get; set; }

        public CallStateMachine(ICall call, IPromptPlayer promptPlayer)
        {
            _call = call;
            _promptPlayer = promptPlayer;
        }

        public void Fire(Trigger trigger)
        {
            StateMachine.Fire(trigger);
        }

        public void SetupFiniteStateMachine()
        {
            InitializeStateMachine();

            StateMachine.Configure(State.CallFlow)
                .Permit(Trigger.PlayPrompt, State.PlayingPrompt)
                .Permit(Trigger.PlayInterruptiblePrompt, State.PlayingInterruptiblePrompt)
                .Permit(Trigger.CaptureInput, State.CapturingInput)
                .Permit(Trigger.PlaceOnHold, State.OnHold)
                .Permit(Trigger.PlayLanguagePrompts, State.LanguagePrompts)
                .PermitReentry(Trigger.NextCallFlowStep)
                .PermitReentry(Trigger.FailedInputCapture)
                .Ignore(Trigger.FinishedPrompt)
                .Ignore(Trigger.PlayNextPrompt)
                .Permit(Trigger.FailedCallFlow, State.HangUp)
                .Permit(Trigger.FinishCall, State.HangUp)
                .Permit(Trigger.StartRecording, State.StartingRecording)
                .OnEntry(_call.ProcessCallLogic);

            StateMachine.Configure(State.OnHold)
                .Permit(Trigger.FinishCall, State.HangUp)
                .Permit(Trigger.NextCallFlowStep, State.CallFlow)
                .Ignore(Trigger.FinishedPrompt);

            SetupPromptStates();
            SetupDigitCaptureEvents();

            StateMachine.Configure(State.HangUp)
                .Ignore(Trigger.FinishCall)
                .Ignore(Trigger.PlayInterruptiblePrompt)
                .Ignore(Trigger.PlayPrompt)
                .Ignore(Trigger.PlayNextPrompt)
                .Ignore(Trigger.FailedCallFlow)
                .Ignore(Trigger.FinishedPrompt)
                .Ignore(Trigger.StartCallFlow)
                .Ignore(Trigger.PlaceOnHold)
                .Ignore(Trigger.PlayLanguagePrompts)
                .PermitIf(Trigger.NextCallFlowStep, State.CallFlow, () => !_call.CallState.CallCanBeAbandoned, "Cannot abandon call yet.")
                .IgnoreIf(Trigger.NextCallFlowStep, () => _call.CallState.CallCanBeAbandoned, "Abandoning Call")
                .Ignore(Trigger.CaptureInput)
                .OnEntry(_call.Hangup);

            StateMachine.Configure(State.InCall);

            StateMachine.Configure(State.StartingRecording)
                .Permit(Trigger.NextCallFlowStep, State.CallFlow)
                .Permit(Trigger.FinishCall, State.HangUp);
        }

        private void SetupDigitCaptureEvents()
        {
            StateMachine.Configure(State.CapturingInput)
                .Permit(Trigger.InputReceived, State.CallFlow)
                .Permit(Trigger.NextCallFlowStep, State.CallFlow)
                .Permit(Trigger.FailedInputCapture, State.CallFlow)
                .Permit(Trigger.FinishCall, State.HangUp)
                .Ignore(Trigger.FinishedPrompt)
                .Ignore(Trigger.PlayNextPrompt)
                .Ignore(Trigger.PromptInterrupted);
        }

        private void InitializeStateMachine()
        {
            StateMachine = new StateMachine<State, Trigger>(State.OffHook);

            StateMachine.Configure(State.OffHook)
                .Permit(Trigger.Answered, State.Initialization);

            StateMachine.Configure(State.Initialization)
                .Permit(Trigger.BadConfig, State.HangUp)
                .Permit(Trigger.StartCallFlow, State.CallFlow);

            StateMachine.Configure(State.LanguagePrompts)
                .Permit(Trigger.GetLanguageInput, State.LanguageInput)
                .Permit(Trigger.NextCallFlowStep, State.CallFlow)
                .Permit(Trigger.FailedCallFlow, State.HangUp)
                .Permit(Trigger.FinishCall, State.HangUp)
                .OnEntry(_call.ProcessCallLogic);

            StateMachine.Configure(State.LanguageInput)
                .Permit(Trigger.PlayLanguagePrompts, State.LanguagePrompts)
                .Permit(Trigger.NextCallFlowStep, State.CallFlow)
                .Permit(Trigger.FailedCallFlow, State.HangUp)
                .Permit(Trigger.FinishCall, State.HangUp);
        }

        private void SetupPromptStates()
        {
            StateMachine.Configure(State.PlayingInterruptiblePrompt)
                .PermitReentry(Trigger.PlayNextPrompt)
                .Permit(Trigger.FinishedPrompt, State.CallFlow)
                .Permit(Trigger.PromptInterrupted, State.StoppingPlayback)
                .Permit(Trigger.FinishCall, State.HangUp)
                .Permit(Trigger.InputReceived, State.CallFlow)
                .Ignore(Trigger.PlayInterruptiblePrompt)
                .OnEntry(_promptPlayer.PlayPromptsInQueue);

            StateMachine.Configure(State.StoppingPlayback)
                .Permit(Trigger.FinishedPrompt, State.CallFlow);

            StateMachine.Configure(State.PlayingPrompt)
                .PermitReentry(Trigger.PlayNextPrompt)
                .Ignore(Trigger.NextCallFlowStep)
                .Permit(Trigger.FinishedPrompt, State.CallFlow)
                .Permit(Trigger.FinishCall, State.HangUp)
                .OnEntry(_promptPlayer.PlayPromptsInQueue);
        }
    }
}