using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Arke.SipEngine.Api
{
    public delegate Task RecordingFinishedEventHandler(ISipApiClient sender, RecordingFinishedEventHandlerArgs args);

    public class RecordingFinishedEventHandlerArgs
    {
        public string RecordingName { get; set; }
    }
}
