using System;
using System.IO.Ports;

namespace Ecorise.Sensor.PMSensor
{
    public class NovaSDS011SensorEventArgs : EventArgs
    {
        public NovaSDS011SensorEventArgs(double pm25, double pm10) { PM25 = pm25; PM10 = pm10; ; }
        public double PM25 { get; private set; }
        public double PM10 { get; private set; }
    }

    public class NovaSDS011SensorDevice : IDisposable
    {
        private SerialPort serialPort;
        public event EventHandler<NovaSDS011SensorEventArgs> DataReceived;

        public NovaSDS011SensorDevice()
        {
            serialPort = null;
        }

        public void Open(string serialPortName)
        {
            if (IsOpen)
            {
                Close();
            }

            serialPort = new SerialPort(serialPortName)
            {
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                ReadTimeout = 500
            };

            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPortDataReceived);
            serialPort.Open();
        }

        public void Close()
        {
            if (serialPort != null)
            {
                serialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialPortDataReceived);
                serialPort.Close();
                serialPort = null;
            }
        }

        public bool IsOpen => (serialPort != null) ? serialPort.IsOpen : false;

        public SerialPort ComPort
        {
            get => serialPort;

            set
            {
                if (serialPort != value)
                {
                    Close();
                    serialPort = value;
                }
            }
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialPort.BytesToRead > 0)
            {
                byte[] data = new byte[20];
                int count = serialPort.Read(data, 0, 20);

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
                            DataReceived?.Invoke(this, new NovaSDS011SensorEventArgs(pm25, pm10));

                            // Selon http://inovafitness.com/software/SDS011%20laser%20PM2.5%20sensor%20specification-V1.3.pdf
                            // la précision est de +/-15% +/-10μg/m³
                        }
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
