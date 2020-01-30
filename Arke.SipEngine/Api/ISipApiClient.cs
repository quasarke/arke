namespace Arke.SipEngine.Api
{
    public interface ISipApiClient
    {
        event DtmfReceivedEventHandler OnDtmfReceivedEvent;
        event PromptPlaybackFinishedEventHandler OnPromptPlaybackFinishedAsyncEvent;
        event LineHangupEventHandler OnLineHangupAsyncEvent;
        event Arke.SipEngine.Api.RecordingFinishedEventHandler OnRecordingFinishedAsyncEvent;
    }
}