using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Ecorise.Utils
{
    public class Logger
    {
        private string filenamePrefix;
        private StringBuilder sbLog;
        private string LogFilePath;
        private DateTime nextFileCreationTime;

        public Logger(string prefix)
        {
            filenamePrefix = prefix;
            sbLog = new StringBuilder();
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

            // Debug.WriteLine(LogFilePath);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Ne pas supprimer d'objets plusieurs fois")]
        protected void InternalLog(string message)
        {
            try
            {
                using (FileStream fs = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    using (StreamWriter sw = new StreamWriter(fs/*, Encoding.UTF8*/))
                    {
                        sw.WriteLine(message);
                    }
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

        public void Log(string format, params object[] args)
        {
            // Format incoming arguments
            string formatted = string.Format(format, args);

            // Search for a new line
            int indexNewLine = formatted.LastIndexOf('\n');

            // We write a "line" only when a new line is encountered
            if (indexNewLine != -1)
            {
                // Extract everything up to the new line character
                sbLog.Append(formatted.Substring(0, indexNewLine));

                string output = sbLog.ToString();

                // Write extracted substring to the console and the file
                Console.WriteLine(output);
                InternalLog(output);

                // Clear what we just wrote
                sbLog.Clear();
            }

            // Store remaining substring, if any
            sbLog.Append(formatted, indexNewLine + 1, formatted.Length - indexNewLine - 1);
        }

        public void LogDateTime(DateTime utcNow)
        {
            DateTime now = utcNow.ToLocalTime();
            Log(String.Format("{0:00}.{1:00}.{2:0000} {3:00}:{4:00}:{5:00}", now.Day, now.Month, now.Year, now.Hour, now.Minute, now.Second));
        }
    }
}
