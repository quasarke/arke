using System;

namespace Arke.SipEngine.CallObjects.RecordingFiles
{
    public class ArkeRecordingFilenameGenerator: RecordingFileNameGenerator
    {
        public override string GetRecordingFileName(string engineIdentifier, string engineUniqueIdSuffix,string facilityUniqueIndentifier)
        {
          return GenerateFileName(engineIdentifier,engineUniqueIdSuffix,facilityUniqueIndentifier);
        }

        private string GenerateFileName(string engineIdentifier, string engineUniqueIdSuffix,string facilityUniqueIdentifier)
        {
            var currentDateTime = DateTime.Now;
            var dateFormat = currentDateTime.ToString("yyyyMMddHHmmss");
            return $"{engineIdentifier}{engineUniqueIdSuffix}{facilityUniqueIdentifier}{dateFormat}";
        }

    }
        
}
