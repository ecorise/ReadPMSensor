using System;
using System.IO.Ports;

namespace Ecorise.Sensor.PMSensor
{
    public class NovaSds011SensorEventArgs : EventArgs
    {
        public NovaSds011SensorEventArgs(double pm25, double pm10) { PM25 = pm25; PM10 = pm10; ; }
        public double PM25 { get; private set; }
        public double PM10 { get; private set; }
    }

    public class NovaSds011SensorDevice : IDisposable
    {
        private SerialPort serialPort;
        private string portName;
        public event EventHandler<NovaSds011SensorEventArgs> DataReceived;

        public NovaSds011SensorDevice()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Supprimer les objets avant la mise hors de portée")]
        public bool Open(string serialPortName)
        {
            if (IsOpen && (serialPortName != portName))
            {
                Close();
            }

            portName = serialPortName;

            try
            {
                serialPort = new SerialPort(serialPortName)
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    ReadTimeout = 500
                };

                serialPort.DataReceived += SerialPortDataReceived;

                serialPort.Open();
            }
            catch (System.IO.IOException)
            {
                // Ignore "com port does not exist"
                serialPort.Dispose();
                serialPort = null;
                return false;
            }

            return serialPort.IsOpen;
        }

        protected void Reopen()
        {
            Close();
            Open(portName);
        }

        public void Close()
        {
            if (serialPort != null)
            {
                serialPort.DataReceived -= SerialPortDataReceived;
                serialPort.Close();
                serialPort = null;
            }
        }

        public bool IsOpen => (serialPort != null) ? serialPort.IsOpen : false;

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialPort.BytesToRead > 0)
            {
                byte[] data = new byte[20];
                int count = 0;

                try
                {
                    count = serialPort.Read(data, 0, 20);

                    if (count == 10) // Protocol specifies 10 bytes
                    {
                        if (data[0] == 0xAA && data[1] == 0xC0)
                        {
                            double pm25 = ((data[3] * 256) + data[2]) / 10.0;
                            double pm10 = ((data[5] * 256) + data[4]) / 10.0;

                            byte checksum = 0;
                            for (int i = 2; i < 8; i++)
                            {
                                checksum += data[i];
                            }

                            if (checksum == data[8])
                            {
                                DataReceived?.Invoke(this, new NovaSds011SensorEventArgs(pm25, pm10));

                                // Selon http://inovafitness.com/software/SDS011%20laser%20PM2.5%20sensor%20specification-V1.3.pdf
                                // la précision est de +/-15% +/-10μg/m³
                            }
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // Port closed
                    try
                    {
                        Reopen();
                    }
                    catch (InvalidOperationException)
                    {
                        // Ignore "com port does not exist"
                        return;
                    }
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
                    serialPort?.Close();
                    portName = null;
                    DataReceived = null;
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
