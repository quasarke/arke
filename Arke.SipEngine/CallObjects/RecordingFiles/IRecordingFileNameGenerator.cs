namespace Arke.SipEngine.CallObjects.RecordingFiles
{
    interface IRecordingFileNameGenerator
    {
        string GetRecordingFileName(string engineIdentifier, string engineUniqueIdentifier,string facililtyUniqueIdentifier);

    }
}
