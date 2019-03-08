namespace Arke.SipEngine.CallObjects.RecordingFiles
{


    public class ArkeFileFormatFactory : RecordingFileFormatFactory
    {
        public override string GetFileFormat()
        {
            return "ulaw";
        }
    }
}
