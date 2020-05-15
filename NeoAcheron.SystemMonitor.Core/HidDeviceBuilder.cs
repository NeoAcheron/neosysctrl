using HidSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{
    public abstract class HidDeviceBuilder {

        public readonly int VendorId;
        public readonly int ProductId;

        public HidDeviceBuilder(int vendorId, int productId)
        {
            VendorId = vendorId;
            ProductId = productId;
        }

        public abstract HidBase BuildInstance(HidDevice hidDevice);
    }

    public class HidDeviceBuilder<T> : HidDeviceBuilder where T : HidBase, new()
    {
        public HidDeviceBuilder(int vendorId, int productId) : base(vendorId, productId) { }

        public override HidBase BuildInstance(HidDevice hidDevice)
        {
            HidBase newDevice = new T();
            newDevice.Initialize(hidDevice);
            return newDevice;
        }
    }
}
