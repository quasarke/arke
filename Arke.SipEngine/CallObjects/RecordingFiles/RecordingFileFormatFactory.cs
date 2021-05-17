using System.Collections.Generic;

namespace Arke.SipEngine.CallObjects.RecordingFiles
{ 

    public abstract class RecordingFileFormatFactory
    {
        public List<string> ValidCallEngines = new List<string>
        {     
            "Arke"
        };
        public abstract string GetFileFormat();
    }
}
