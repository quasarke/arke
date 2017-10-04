using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arke.SipEngine.Api;
using Arke.SipEngine.CallObjects;

namespace Arke.IVR.Recording
{
    public class ArkeRecordingManager : IRecordingManager
    {
        private readonly ISipRecordingApi _ariClient;
        private readonly Dictionary<string, string> _recordingsInProgress = new Dictionary<string, string>();
        private readonly string _creationDateTime;

        public ArkeRecordingManager(ISipRecordingApi ariClient)
        {
            _ariClient = ariClient;
            _creationDateTime = DateTime.Now.ToString("yyyyMMddHHmmss").ToString();
        }

        public async Task StartRecordingOnLine(string lineId, string direction, ICallInfo callState)
        {
            var recordingId = await _ariClient.StartRecordingOnLine(
                lineId, GetFileName(lineId, direction, callState));

            _recordingsInProgress.Add(lineId, recordingId);
        }

        public async Task StopRecordingOnLine(string lineId)
        {
            if (_recordingsInProgress.ContainsKey(lineId))
            {
                await _ariClient.StopRecording(_recordingsInProgress[lineId]);
                _recordingsInProgress.Remove(lineId);
            }
        }

        public async Task StartRecordingOnBridge(string bridgeId, ICallInfo callState)
        {
            var recordingId = await _ariClient.StartRecordingOnBridge(
                bridgeId, GetFileName(bridgeId, "B", callState));
            _recordingsInProgress.Add(bridgeId, recordingId);
        }

        public async Task StopRecordingOnBridge(string bridgeId)
        {
            if (_recordingsInProgress.ContainsKey(bridgeId))
            {
                await _ariClient.StopRecording(_recordingsInProgress[bridgeId]);
                _recordingsInProgress.Remove(bridgeId);
            }
        }

        public string GetFileName(string lineId, string direction, ICallInfo callState)
        {
            return $"{ArkeCallFlowService.Configuration.GetSection("appSettings:ArkeServerID").Value}_{lineId}_{_creationDateTime}_{direction}";
        }

        public async Task StopAllRecordings()
        {
            foreach (var recordingItem in _recordingsInProgress)
            {
                await _ariClient.StopRecording(recordingItem.Value);
                _recordingsInProgress.Remove(recordingItem.Key);
            }
        }
    }
}