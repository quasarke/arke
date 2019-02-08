namespace Arke.SipEngine.Api
{
    public interface ISipApiClient
    {
        event DtmfReceivedEventHandler OnDtmfReceivedEvent;
        event PromptPlaybackFinishedEventHandler OnPromptPlaybackFinishedEvent;
        event LineHangupEventHandler OnLineHangupEvent;
        event Arke.SipEngine.Api.RecordingFinishedEventHandler OnRecordingFinishedEvent;
    }
}