using System.Threading;

namespace Arke.SipEngine.CallObjects
{
    public interface IPhoneInputHandler
    {
        string DigitsReceived { get; set; }
        Timer DigitTimeoutTimer { get; set; }
        int MaxDigitTimeoutInSeconds { get; set; }
        int NumberOfDigitsToWaitForNextStep { get; set; }
        void StartUserInput(bool reset);
        void ChangeInputSettings(PhoneInputHandlerSettings settings);
    }
}