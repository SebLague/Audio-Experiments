using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System;

namespace Seb.Helpers
{
    public static class FileHelper
    {
        public static void SaveFile(string directoryPath, string filename, string data, bool printSavePath = false)
        {
            string path = Path.Combine(directoryPath, filename);
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(path, data);
            if (printSavePath)
            {
                UnityEngine.Debug.Log(path);
            }
        }

        public static string ReadAllText(params string[] pathStrings) => File.ReadAllText(Path.Combine(pathStrings));

        public static string ProjectDirectory => Directory.GetCurrentDirectory();

        public static string LocalApplicationDataDirectory => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string GetParentDirectory(string directoryPath, int numLevels = 1)
        {
            for (int i = 0; i < numLevels; i++)
            {
                directoryPath = Directory.GetParent(directoryPath).FullName;
            }

            return directoryPath;
        }


        public static string GetUniqueFileName(string path, string fileName, string fileExtension, bool appendZeroIfAlreadyUnique = false)
        {
            if (fileExtension[0] != '.')
            {
                fileExtension = "." + fileExtension;
            }

            int index = 0;
            string uniqueName = fileName;
            if (appendZeroIfAlreadyUnique) uniqueName += index;

            while (File.Exists(Path.Combine(path, uniqueName + fileExtension)))
            {
                index++;
                uniqueName = fileName + index;
            }

            return uniqueName + fileExtension;
        }


        // Thanks to https://github.com/dotnet/runtime/issues/17938
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}