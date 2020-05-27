using HidSharp;
using NeoAcheron.SystemMonitor.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace NeoAcheron.SystemMonitor.Corsair.PowerSupplies
{
    public class SeriesRMi : HidBase, IMeasurable
    {
        public string Name { get; private set; }

        public enum PsuCommandCode : byte
        {
            PAGE = 0,
            FAN_CONFIG_1_2 = 58, // 0x3A
            FAN_COMMAND_1 = 59, // 0x3B
            VOUT_OV_FAULT_LIMIT = 64, // 0x40
            VOUT_UV_FAULT_LIMIT = 68, // 0x44
            IOUT_OC_FAULT_LIMIT = 70, // 0x46
            OT_FAULT_LIMIT = 79, // 0x4F
            STATUS_VOUT = 122, // 0x7A
            STATUS_IOUT = 123, // 0x7B
            STATUS_TEMPERATURE = 125, // 0x7D
            STATUS_CML = 126, // 0x7E
            STATUS_FANS_1_2 = 129, // 0x81
            READ_VIN = 136, // 0x88
            READ_VOUT = 139, // 0x8B
            READ_IOUT = 140, // 0x8C
            READ_TEMPERATURE_1 = 141, // 0x8D
            READ_TEMPERATURE_2 = 142, // 0x8E
            READ_FAN_SPEED_1 = 144, // 0x90
            READ_POUT = 150, // 0x96
            READ_MFR_ID = 153, // 0x99
            READ_MFR_MODEL = 154, // 0x9A
            READ_RUNTIME_TOTAL = 209, // 0xD1
            READ_RUNTIME_NOW = 210, // 0xD2
            READ_FIRMWARE_REVISION = 212, // 0xD4
            MFR_12V_OCP_MODE = 216, // 0xD8
            SET_BLACKBOX_MODE = 217, // 0xD9
            MFR_RESET_USER_SETTING = 221, // 0xDD
            MFR_READ_TOTAL_POUT = 238, // 0xEE
            FAN_INDEX = 240, // 0xF0
            HANDSHAKE = 254, // 0xFE
        }

        private readonly Measurement VrmTemperature = new Measurement("VrmTemperature");
        private readonly Measurement ControllerTemperature = new Measurement("ControllerTemperature");
        private readonly Measurement FanSpeed = new Measurement("FanSpeed");
        private readonly Measurement PowerIn = new Measurement("PowerIn");
        private readonly Measurement PowerOut = new Measurement("PowerOut");

        private readonly List<PsuCommandCode> UpdateSequence = new List<PsuCommandCode>()
        {
            
        };

        public Measurement[] AllMeasurements => new Measurement[] {
            VrmTemperature,
            ControllerTemperature,
            FanSpeed,
            PowerIn,
            PowerOut
        };

        public SeriesRMi() { }

        protected override void Start()
        {
            byte[] buffer = new byte[HID_BUFFER_SIZE];

            //while(Name == null)
            //{
            //    byte[] buffer = new byte[64];
            //    buffer[0] = (byte)PsuCommandCode.HANDSHAKE;
            //    buffer[1] = (byte)0x03;
            //    buffer[2] = (byte)commandCode;

            //    WriteCommand(PsuCommandCode.HANDSHAKE);
            //    WriteCommand(PsuCommandCode.READ_MFR_MODEL);
            //    Read(buffer);
            //    Accept(buffer);
            //}

            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.AutoReset = true;
            timer.Elapsed += PollStatus;
            timer.Enabled = true;
        }

        private void PollStatus(object sender, ElapsedEventArgs e)
        {
            WriteCommand(PsuCommandCode.READ_TEMPERATURE_1);
            WriteCommand(PsuCommandCode.READ_TEMPERATURE_2);
            WriteCommand(PsuCommandCode.MFR_READ_TOTAL_POUT);
            WriteCommand(PsuCommandCode.READ_FAN_SPEED_1);

        }

        private void WriteCommand(PsuCommandCode commandCode)
        {
            byte[] buffer = new byte[64];
            buffer[0] = (byte)PsuCommandCode.HANDSHAKE;
            buffer[1] = (byte)0x03;
            buffer[2] = (byte)commandCode;

            Write(buffer);
        }

        protected override void Stop()
        {
        }

        protected override void Accept(byte[] data)
        {
            PsuCommandCode command = (PsuCommandCode) data[2];
            switch(command){
                case PsuCommandCode.READ_MFR_MODEL:
                    string model = System.Text.Encoding.ASCII.GetString(data, 3, data.Length - 3).Trim('\0');
                    Name = "Corsair/" + model;
                    break;
                case PsuCommandCode.READ_TEMPERATURE_1:
                    VrmTemperature.Value = (get_float_data(data));
                    break;
                case PsuCommandCode.READ_TEMPERATURE_2:
                    ControllerTemperature.Value = (get_float_data(data));
                    break;
                case PsuCommandCode.MFR_READ_TOTAL_POUT:
                    PowerOut.Value = (get_float_data(data));
                    break;
                case PsuCommandCode.READ_FAN_SPEED_1:
                    FanSpeed.Value = (get_float_data(data));
                    break;
            }
        }

        private object get_float_data(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
