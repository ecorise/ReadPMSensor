using System;
using System.Diagnostics;
using System.IO;

namespace Ecorise.Utils
{
    public class Logger
    {
        private string filenamePrefix;
        private string LogFilePath;
        private DateTime nextFileCreationTime;

        public Logger(string prefix)
        {
            filenamePrefix = prefix;
            CreateNewFileNow();
        }

        private void CreateNewFileNow()
        {
            DateTime now = DateTime.UtcNow;

            nextFileCreationTime = now + new TimeSpan(12, 19, 3, 0); // Creates a new file after twelve days, nineteen hours and three minutes
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            LogFilePath = basePath + @"\OneDrive\Documents\" + filenamePrefix + "_" + now.ToLocalTime().ToString("yyMMdd_HHmm") + ".txt";

            try
            {
                // Overwrite any existing file
                using (File.Create(LogFilePath))
                {
                }
            }
            catch (UnauthorizedAccessException e)
            {
                // Path is not accessible
                Debug.WriteLine("Error creating log file: " + e.ToString());
            }

            Console.WriteLine(LogFilePath);
        }

        public void Log(string message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read)/*, Encoding.UTF8*/))
                {
                    sw.WriteLine(message);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                // Path is not accessible
                Debug.WriteLine("Error logging: " + e.ToString());
            }
            catch (IOException e)
            {
                Debug.WriteLine("Error logging: " + e.ToString());
            }

            if (DateTime.UtcNow > nextFileCreationTime)
            {
                CreateNewFileNow();
            }
        }
    }
}
