namespace Arke.SipEngine.Api
{
    public interface ISipApiClient
    {
        event DtmfReceivedEventHandler OnDtmfReceivedEvent;
        event PromptPlaybackFinishedEventHandler OnPromptPlaybackFinishedAsyncEvent;
        event LineHangupEventHandler OnLineHangupAsyncEvent;
        event Arke.SipEngine.Api.RecordingFinishedEventHandler OnRecordingFinishedAsyncEvent;

        public void SubscribeToDtmfReceivedEvents();
        public void SubscribeToPlaybackFinishedEvents();
        public void UnsubscribeToDtmfReceivedEvents();
        public void UnsubscribeToPlaybackFinishedEvents();
        public void SubscribeToLineHangupEvents();
        public void SetAppNameForEvents(string appName);
    }
}