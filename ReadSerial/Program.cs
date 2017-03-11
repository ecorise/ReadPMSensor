using System;
using System.Threading;
using Ecorise.Sensor.PMSensor;
using Ecorise.Utils;

namespace ReadPMSensor
{
    public class Program : IDisposable
    {
        enum Mode
        {
            Init,
            Running,
            Exit
        };

        private readonly string serialInputDeviceComPort = "com3";
        private NovaSds011SensorDevice sensorDevice;
        private Logger log = new Logger("PM");

        void Run()
        {
            log.Log("Date et heure\tPM2.5 [ug/m3]\tPM10 [ug/m3]\n");

            Mode mode = Mode.Init;

            while (mode != Mode.Exit)
            {
                if (mode == Mode.Running)
                {
                    if (!sensorDevice.IsOpen)
                    {
                        mode = Mode.Init;
                    }
                }
                else if (mode == Mode.Init)
                {
                    sensorDevice = new NovaSds011SensorDevice();

                    if (sensorDevice.Open(serialInputDeviceComPort))
                    {
                        sensorDevice.DataReceived += PMSensorDataReceived;
                        mode = Mode.Running;
                    }
                    else
                    {
                        DateTime now = DateTime.UtcNow;
                        log.LogDateTime(now);
                        log.Log($"\tImpossible d'ouvrir le port de communication {serialInputDeviceComPort} !\n");
                        Thread.Sleep(900);
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

            sensorDevice.DataReceived -= PMSensorDataReceived;
            sensorDevice?.Close();
        }

        public void PMSensorDataReceived(object sender, NovaSds011SensorEventArgs e)
        {
            if (e != null)
            {
                log.LogDateTime(DateTime.UtcNow);
                log.Log("\t{0:0.0}\t{1:0.0}\n", e.PM25, e.PM10);
            }
        }

        public static void Main()
        {
            using(new PreventSleepMode())
            {
                using (Program program = new Program())
                {
                    program.Run();
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sensorDevice.Close();
                    sensorDevice.Dispose();
                    sensorDevice = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
