using System;
using System.Text;
using System.Threading;
using Ecorise.Equipment.PMSensor;
using Ecorise.Utils;

namespace ReadPMSensor
{
    public enum Mode
    {
        Init,
        Running,
        Exit
    };

    public class Program
    {
        PMSensorDevice pmSensorDevice;

        private Logger log = new Logger("PM");
        private StringBuilder sbLog = new StringBuilder();
        private StringBuilder sbConsole = new StringBuilder();

        private string serialInputDeviceComPort = "com3";

        void Run()
        {
            Output(log, "Date et heure\tPM2.5 [μg/m³]\tPM10 [μg/m³]");
            Output(log, "", true);

            Mode mode = Mode.Init;

            while (mode != Mode.Exit)
            {
                if (mode == Mode.Running)
                {
                }
                else if (mode == Mode.Init)
                {
                    pmSensorDevice = new PMSensorDevice();
                    pmSensorDevice.Open(serialInputDeviceComPort);

                    if (pmSensorDevice.IsOpen)
                    {
                        Console.WriteLine($"Le port de communication {serialInputDeviceComPort} est ouvert.");
                        Console.WriteLine("");

                        pmSensorDevice.DataReceived += PMSensorDataReceived;

                        mode = Mode.Running;
                    }
                    else
                    {
                        DateTime now = DateTime.UtcNow;
                        LogDateTime(now);
                        Output(log, String.Format($"Impossible d'ouvrir le port de communication {serialInputDeviceComPort} !"), true);
                        Thread.Sleep(500);
                    }
                }

                if (Console.KeyAvailable)
                {
                    char ch = Console.ReadKey(true).KeyChar;

                    switch (ch)
                    {
                        case 'q':
                            mode = Mode.Exit;
                            break;

                        default:
                            Console.WriteLine("Appuyez sur q pour quitter...");
                            break;
                    }

                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                    }
                }

                Thread.Sleep(100);
            }

            pmSensorDevice.DataReceived -= PMSensorDataReceived;
            pmSensorDevice?.Close();
        }

        private void Output(Logger log, string s, bool endOfLine = false, bool doNotDisplayOnConsole = false)
        {
            sbLog.Append(s);

            if (!doNotDisplayOnConsole)
            {
                sbConsole.Append(s);
            }

            if (endOfLine)
            {
                Console.WriteLine(sbConsole.ToString());
                log.Log(sbLog.ToString());
                sbLog.Clear();
                sbConsole.Clear();
            }
        }

        private void LogDateTime(DateTime utcNow)
        {
            DateTime now = utcNow.ToLocalTime();
            Output(log, String.Format("{0:00}.{1:00}.{2:0000} {3:00}:{4:00}:{5:00}\t", now.Day, now.Month, now.Year, now.Hour, now.Minute, now.Second));
        }

        public void PMSensorDataReceived(object sender, PMSensorEventArgs e)
        {
            if (e != null)
            {
                LogDateTime(DateTime.UtcNow);
                Output(log, string.Format("{0}\t", e.PM25));
                Output(log, string.Format("{0}\t", e.PM10));
                Output(log, "", true);
            }
        }

        public static void Main()
        {
            // Added to prevent sleep mode. See http://stackoverflow.com/questions/6302185/how-to-prevent-windows-from-entering-idle-state
            uint fPreviousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);

            if (fPreviousExecutionState == 0)
            {
                Console.WriteLine("SetThreadExecutionState failed.");
            }

            try
            {
                new Program().Run();
            }
            finally
            {
                try
                {
                    // Restore previous state
                    NativeMethods.SetThreadExecutionState(fPreviousExecutionState);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
