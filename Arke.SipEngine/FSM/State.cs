namespace Arke.SipEngine.FSM
{
    public enum State
    {
        OffHook,
        Initialization,
        LanguagePrompts,
        LanguageInput,
        CallFlow,
        InCall,
        OnHold,
        HangUp,
        PlayingPrompt,
        PlayingInterruptiblePrompt,
        CapturingInput,
        StoppingPlayback,
        StartingRecording,
        StoppingRecording
    }
}
