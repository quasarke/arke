using System;

namespace Arke.SipEngine.Prompts
{
    public class LanguageData
    {
        public byte Id { get; set; }
        public string ShortName { get; set; }
        public string FolderName { get; set; }

        public LanguageData(string shortCode)
        {
            switch (shortCode)
            {
                case "en":
                    ShortName = "enUS";
                    FolderName = "en";
                    break;
                case "es":
                    ShortName = "esMX";
                    FolderName = "es";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shortCode), shortCode,
                        "Invalid Language, support not found.");
            }

        }
    }
}
