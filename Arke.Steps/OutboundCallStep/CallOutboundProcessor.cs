using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Arke.DSL.Step;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using Arke.SipEngine.Processors;
using PhoneNumbers;

namespace Arke.Steps.OutboundCallStep
{
    public class CallOutboundProcessor : IStepProcessor
    {
        private const string NextStep = "NextStep";
        private const string FailedStep = "Error";
        private const string NoAnswer = "NoAnswer";
        private ICall _call;
        private Step _step;
        public string Name => "CallOutbound";

        public async Task DoStep(Step step, ICall call)
        {
            _step = step;
            _call = call;
            await CallOutbound(_call.CallState.Destination).ConfigureAwait(false);
        }

        public void GoToNextStep()
        {
            var next = _step.GetStepFromConnector(NextStep);
            _call.Logger.Information("Outbound connected, go to next step {stepId} {@Call}", next, _call.CallState);
            _call.CallState.AddStepToOutgoingQueue(next);
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        public void GoToFailStep()
        {
            _call.CallState.AddStepToOutgoingQueue(_step.GetStepFromConnector(FailedStep));
            _call.FireStateChange(Trigger.NextCallFlowStep);
        }

        public async Task CallOutbound(string dialString)
        {
            _call.Logger.Information("Outbound start " + dialString);
            while (_call.CallState.OutboundEndpoint.Count > 0)
            {
                if (await PlaceOutboundCall(dialString)) return;
            }
            
            // give the person a moment to get the phone to their ear. We were missing quite a bit of the first prompt.
            await Task.Delay(1000);
            _call.CallState.CalledPartyAnswerTime = DateTimeOffset.Now;
            GoToNextStep();
        }

        private async Task<bool> PlaceOutboundCall(string dialString)
        {
            var outboundEndpoint = GetOutboundEndpoint();
            try
            {
                var outgoingLineId = await AttemptOutboundCall(outboundEndpoint, dialString, "9044950107");
                var currentCallState = await _call.SipLineApi.GetLineState(outgoingLineId).ConfigureAwait(false);

                var noAnswerTimeout = new Stopwatch();
                noAnswerTimeout.Start();
                if (await WaitForCallToConnect(currentCallState, noAnswerTimeout, outgoingLineId))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _call.Logger.Error(ex, $"Dial through carrier {outboundEndpoint} failed. Trying next carrier.");
            }
            return false;
        }

        private async Task<bool> WaitForCallToConnect(string currentCallState, Stopwatch noAnswerTimeout, string outgoingLineId)
        {
            while (currentCallState != "Up")
            {
                if (noAnswerTimeout.Elapsed.TotalSeconds > 90)
                {
                    _call.CallState.AddStepToOutgoingQueue(_step.GetStepFromConnector(NoAnswer));
                    _call.FireStateChange(Trigger.NextCallFlowStep);
                    return true;
                }

                await Task.Delay(100).ConfigureAwait(false);
                currentCallState = await _call.SipLineApi.GetLineState(outgoingLineId).ConfigureAwait(false);
            }

            return false;
        }

        private async Task<string> AttemptOutboundCall(string outboundEndpoint, string dialString, string callerId)
        {
            dialString = GenerateDialString(dialString);
            dialString = FixOurStupidVendorsDialStringPrefixRequirements(outboundEndpoint, dialString);
            _call.CallState.OutboundUri = outboundEndpoint.Replace("{exten}",dialString);
            _call.CallState.OutboundCallerId = callerId;
            _call.CallState.TrunkOffHookTime = DateTimeOffset.Now;
            var outgoingCall = await _call.SipLineApi.CreateOutboundCall(dialString, callerId, outboundEndpoint)
                .ConfigureAwait(false);
            await Task.Delay(500);
            _call.CallState.CreateOutgoingLine(outgoingCall);
            await Task.Delay(500);
            var outgoingLineId = _call.CallState.GetOutgoingLineId();
            return outgoingLineId;
        }

        private string FixOurStupidVendorsDialStringPrefixRequirements(string outboundEndpoint, string dialString)
        {
            switch (outboundEndpoint)
            {
                case CciTrunkName:
                    return dialString.Replace("+", "");
                case AirespringTrunkName:
                    return dialString;
                case BandwidthIntTrunkName:
                    return dialString;
                case BandwidthUsTrunkName:
                    return dialString;
                case SonusTrunkName:
                    return dialString;
                case ThinqTrunkName:
                    return dialString;
                default:
                    return dialString;
            }
        }

        private string GenerateDialString(string dialString)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var number = phoneNumberUtil.Parse(dialString, "US");
            dialString = phoneNumberUtil.Format(number, PhoneNumberFormat.E164);
            return dialString;
        }

        public string GetOutboundEndpoint()
        {
            return _call.CallState.OutboundEndpoint.Count > 0 ? _call.CallState.OutboundEndpoint.Dequeue() : string.Empty;
        }

        private const string AirespringTrunkName = "PJSIP/{exten}@airespring-trunk";
        private const string BandwidthUsTrunkName = "PJSIP/{exten}@bandwidth-us-trunk";
        private const string BandwidthIntTrunkName = "PJSIP/{exten}@bandwidth-in-trunk";
        private const string SonusTrunkName = "PJSIP/{exten}@sonus-trunk";
        private const string ThinqTrunkName = "PJSIP/{exten}@thinq-trunk";
        private const string CciTrunkName = "PJSIP/{exten}@cci-trunk";
    }
}
