using System;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog;

namespace Arke.SipEngine.BridgeName
{
    public class BridgeNameGenerator
    {
        private static readonly Random Random = new Random();

        public string[] GetAnimalName()
        {
#if DEBUG
            var path = GetAllWords.GetDevFilePathForWordFiles();
            var file = Path.Combine(path, "Animal.txt");
#else
            var file = "Animal.txt";
#endif
            return file.GetWordsFromTextFile();
        }

        public string[] GetActionVerb()
        {
#if DEBUG
            var path = GetAllWords.GetDevFilePathForWordFiles();
            var file = Path.Combine(path, "Verb.txt");
#else
            var file = "Verb.txt";
#endif
            return file.GetWordsFromTextFile();
        }

        public string[] GetNounName()
        {
#if DEBUG
            var path = GetAllWords.GetDevFilePathForWordFiles();
            var file = Path.Combine(path, "Noun.txt");
#else
            var file = "Noun.txt";
#endif
            return file.GetWordsFromTextFile();
        }

        public string GetRandomBridgeName()
        {
            var animal = GetAnimalName();
            var action = GetActionVerb();
            var noun = GetNounName();
            var a = Random.Next(animal.Length);
            var b = Random.Next(action.Length);
            var c = Random.Next(noun.Length);
            var randomAnimal = animal[a].Trim();
            var randomAction = action[b].Trim().ToLowerInvariant();
            var randomNoun = noun[c].Trim();
            return randomAnimal + ' ' + randomAction + ' ' + randomNoun;
        }
    }

    public static class GetAllWords
    {

        public static string[] GetWordsFromTextFile(this string fileName)
        {
            var file = GetFilePathForWordFiles();
            file = Path.Combine(file, fileName);
            var item = "";
            try
            {
                item = File.ReadAllText(file);
            }
            catch (Exception)
            {
            }

            return item.Split(',');
        }

        public static string GetDevFilePathForWordFiles()
        {
            //return "c:\\ArtemisConfig\\";
            var path = PlatformServices.Default.Application.ApplicationBasePath;
            const string textFilesFolder = "TextFilesForBridgeName";
            var workingFolder = path + textFilesFolder;
            var file = Path.Combine(path, textFilesFolder);
            return file;
        }

        private static string GetFilePathForWordFiles()
        {
            var path = PlatformServices.Default.Application.ApplicationBasePath;
            const string textFilesFolder = "TextFilesForBridgeName";
            var workingFolder = path + textFilesFolder;
            var file = Path.Combine(path, textFilesFolder);
            return file;
        }
    }
}
