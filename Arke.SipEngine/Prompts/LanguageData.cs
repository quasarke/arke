using System;

namespace Arke.SipEngine.Prompts
{
    public class LanguageData
    {
        public byte Id { get; set; }
        public string ShortName { get; set; }
        public string FolderName { get; set; }
        public string AzureLanguageCode { get; set; }

        public LanguageData(string shortCode)
        {
            switch (shortCode)
            {
                case "en":
                    ShortName = "enUS";
                    FolderName = "en";
                    AzureLanguageCode = "en-US";
                    break;
                case "es":
                    ShortName = "esMX";
                    FolderName = "es";
                    AzureLanguageCode = "es-MX";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shortCode), shortCode,
                        "Invalid Language, support not found.");
            }

        }
    }
}
