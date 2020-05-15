using HidSharp;
using System;
using System.Threading;

namespace NeoAcheron.SystemMonitor.Core
{

    public abstract class HidBase : IDisposable
    {
        protected const int HID_BUFFER_SIZE = 65;  // See why 65 here https://github.com/jcoenraadts/hid-sharp/wiki

        protected int VendorId { get; private set; }
        protected int ProductId { get; private set; }

        protected readonly Measurement DeviceManufacturer = new Measurement("Manufacturer");
        protected readonly Measurement DeviceProductName = new Measurement("ProductName");
        protected readonly Measurement DeviceSerialNumber = new Measurement("SerialNumber");

        private HidDevice _hidDevice;
        private HidStream _hidStream;

        internal void Initialize(HidDevice hidDevice)
        {
            HidStream hidStream;
            if (hidDevice.TryOpen(out hidStream))
            {
                try
                {
                //    DeviceManufacturer.UpdateValue(this, hidDevice.GetManufacturer());
                }
                catch (Exception ex)
                {
                    // Not all devices give this info
                }
                try
                {
               //     DeviceProductName.UpdateValue(this, hidDevice.GetProductName());
                }
                catch (Exception ex)
                {
                    // Not all devices give this info
                }
                try
                {
                //    DeviceSerialNumber.UpdateValue(this, hidDevice.GetSerialNumber());
                }
                catch (Exception ex)
                {
                    // Not all devices give this info
                }


                _hidDevice = hidDevice;
                _hidStream = hidStream;

                _hidStream.ReadTimeout = 10000;

                Start();
                ThreadPool.UnsafeQueueUserWorkItem(ContinuousRead, false);

            }
        }

        private void ContinuousRead(object state)
        {
            while (_hidStream.CanRead)
            {
                try
                {
                    byte[] data = new byte[HID_BUFFER_SIZE];
                    int readCount = _hidStream.Read(data, 0, HID_BUFFER_SIZE);
                    if (readCount > 0)
                    {
                        Accept(data);
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    /// Some exception handling might be needed here, for now we just sleep for a while
                    Thread.Sleep(1000);
                }
            }
            Stop();
        }

        protected int Read(byte[] buffer)
        {
            return _hidStream.Read(buffer);
        }

        protected void Write(byte[] buffer)
        {
            _hidStream.Write(buffer);
        }

        protected abstract void Start();
        protected abstract void Stop();

        protected abstract void Accept(byte[] data);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                    _hidStream.Close();

                    _hidStream = null;
                    _hidDevice = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        public static byte[] Pad(params byte[] data)
        {
            byte[] buffer = new byte[HID_BUFFER_SIZE];
            Buffer.BlockCopy(data, 0, buffer, 0, Math.Min(data.Length, buffer.Length));
            return buffer;
        }
    }
}
