namespace Arke.SipEngine.CallObjects.RecordingFiles
{
    public abstract class RecordingFileNameGenerator : IRecordingFileNameGenerator
    {
        public abstract string GetRecordingFileName(string engineIdentifier, string engineUniqueIdentifier, string facililtyUniqueIdentifier);
    }
}
