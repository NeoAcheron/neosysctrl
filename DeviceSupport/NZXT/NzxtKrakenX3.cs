using ColorMine.ColorSpaces;
using HidSharp;
using NeoAcheron.SystemMonitor.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;

namespace NeoAcheron.SystemMonitor.NZXT
{
    public class NzxtKrakenX3 : HidBase, IMeasurable, ISettable
    {
        private static readonly byte[] GET_FIRMWARE_INFO = { 0x10, 0x01 };
        private static readonly byte[] GET_LIGHTING_INFO = { 0x20, 0x03 };
        private static readonly byte[] INITIALIZE_1 = { 0x70, 0x02, 0x01, 0xb8, 0x0b };
        private static readonly byte[] INITIALIZE_2 = { 0x70, 0x01 };
        private static readonly byte[][] SET_PUMP_TARGET_MAP = new byte[101][];

        static NzxtKrakenX3()
        {
            byte[] set_pump_speed_header = { 0x72, 0x01, 0x00, 0x00 };

            for (byte speed = 0; speed < SET_PUMP_TARGET_MAP.Length; speed++)
            {
                SET_PUMP_TARGET_MAP[speed] = set_pump_speed_header.Concat(Enumerable.Repeat(speed, 40)).Concat(new byte[20]).ToArray();
            }
        }

        private readonly Measurement LiquidTemperature = new Measurement("LiquidTemp");
        private readonly Measurement PumpSpeed = new Measurement("PumpSpeed");
        private readonly Measurement PumpTarget = new Measurement("PumpTarget");
        private readonly Measurement FirmwareVersion = new Measurement("FirmwareVersion");

        private readonly Measurement RgbInfo = new Measurement("RgbInfo");

        public readonly Setting PumpTargetSetting = new Setting("PumpTarget", (byte)0x32);
        public readonly Setting RgbControl = new Setting("RgbControl", 0x0);

        public string Name => "Nzxt/KrakenX3";

        public Measurement[] AllMeasurements => new Measurement[]{
            LiquidTemperature,
            PumpSpeed,
            PumpTarget,
            FirmwareVersion,
            RgbInfo
        };

        public Setting[] AllSettings => new Setting[] {
            PumpTargetSetting,
            RgbControl
        };

        protected override void Start()
        {
            Write(GET_FIRMWARE_INFO);
            Write(GET_LIGHTING_INFO);
            Write(INITIALIZE_1);
            Write(INITIALIZE_2);
        }

        protected override void Stop()
        {
        }

        protected override void Accept(byte[] data)
        {
            if (data[0] == 0x75 && data[1] == 0x02)
            {
                UnpackStatusUpdate(data);
            }
            else if (data[0] == 0x11 && data[1] == 0x01)
            {
                UnpackFirmwareInfo(data);
            }
            else if (data[0] == 0x21 && data[1] == 0x03)
            {
                UnpackLightingInfo(data);
            }
        }

        private void UnpackFirmwareInfo(byte[] data)
        {
            FirmwareVersion.UpdateValue(this, String.Format("{0}.{1}.{2}", data[0x11], data[0x12], data[0x13]));
        }

        private void UnpackLightingInfo(byte[] data)
        {
            var num_light_channels = data[14];  //the 15th byte (index 14) is # of light channels
            var accessories_per_channel = 6; //each lighting channel supports up to 6 accessories
            var light_accessory_index = 15; //offset in msg of info about first light accessory

            LedData ledData = new LedData();

            for (byte i = 0; i < num_light_channels; i++)
            {
                List<LedAccessory> accessories = new List<LedAccessory>();
                for (byte j = 0; j < accessories_per_channel; j++)
                {
                    LedAccessoryType accessoryType = (LedAccessoryType)data[light_accessory_index++];
                    if (accessoryType != LedAccessoryType.NotConnected)
                    {
                        LedAccessory ledAccessory = new LedAccessory();
                        ledAccessory.LedAccessoryType = accessoryType;
                        ledAccessory.LedAccessoryIndex = j;
                        ledAccessory.LedAccessoryChannel = i;
                        accessories.Add(ledAccessory);
                    }
                }
                switch (i)
                {
                    case 0:
                        ledData.ExternalLedAccessories = accessories.ToArray();
                        break;
                    case 1:
                        ledData.PumpRingLedAccessories = accessories.ToArray();
                        break;
                    case 2:
                        ledData.PumpLogoLedAccessories = accessories.ToArray();
                        break;
                }
            }

            RgbInfo.UpdateValue(this, ledData);
        }

        private void UnpackStatusUpdate(byte[] data)
        {
            LiquidTemperature.UpdateValue(this, data[15] + data[16] / 10.0f);
            PumpSpeed.UpdateValue(this, (data[18] << 8) | data[17]);
            PumpTarget.UpdateValue(this, data[19]);
        }

        public void SettingUpdate(object source, Setting setting)
        {
            if (setting == PumpTargetSetting)
            {
                byte pwm = (byte)setting.SettingValue;
                if (pwm == 99) pwm = 100; // For some reason, setting it to 99 doesn't work...
                pwm = Math.Clamp(pwm, (byte)0, (byte)100);
                Write(SET_PUMP_TARGET_MAP[pwm]);
            }
            else if (setting == RgbControl)
            {

                ColorSpace[] value = setting.SettingValue as ColorSpace[];
                if (value == null) return;

                int i = 0;

                SuperLedSetting ledSetting = new SuperLedSetting();
                ledSetting.Register = 0x22;
                ledSetting.Opcode = 0x10;
                ledSetting.Channel = 0x02;

                ledSetting.Colour00 = value[i++];
                ledSetting.Colour01 = value[i++];
                ledSetting.Colour02 = value[i++];
                ledSetting.Colour03 = value[i++];
                ledSetting.Colour04 = value[i++];
                ledSetting.Colour05 = value[i++];
                ledSetting.Colour06 = value[i++];
                ledSetting.Colour07 = value[i++];
                ledSetting.Colour08 = value[i++];
                ledSetting.Colour09 = value[i++];
                ledSetting.Colour10 = value[i++];
                ledSetting.Colour12 = value[i++];
                ledSetting.Colour13 = value[i++];
                ledSetting.Colour14 = value[i++];
                ledSetting.Colour15 = value[i++];

                byte[] data = StructUtil.StructureToByteArray(ledSetting);
                Write(data);

                ledSetting = new SuperLedSetting();
                ledSetting.Register = 0x22;
                ledSetting.Opcode = 0x11;
                ledSetting.Channel = 0x02;

                data = StructUtil.StructureToByteArray(ledSetting);
                Write(data);

                data = new byte[] { 0x22, 0xa0, 0x02, 0x00, 0x00, 0x08, 0x00, 0x00, 0x80, 0x00, 0x32, 0x00, 0x00, 0x01 };

                Write(data);
            }
        }

        public enum LedAccessoryType : byte
        {
            NotConnected = 0x00,
            HuePlusLedStrip = 0x01,
            AerRgb1 = 0x02,
            Hue2LedStrip300mm = 0x04,
            Hue2LedStrip250mm = 0x05,
            Hue2LedStrip200mm = 0x06,
            Hue2CableComb = 0x08,
            Hue2Underglow300mm = 0x09,
            Hue2Underglow200mm = 0x0a,
            AerRgb2_120mm = 0x0b,
            AerRgb2_140mm = 0x0c,
            KrakenX3PumpRing = 0x10,
            KrakenX3PumpLogo = 0x11
        }

        public struct LedData
        {
            public LedAccessory[] ExternalLedAccessories;
            public LedAccessory[] PumpRingLedAccessories;
            public LedAccessory[] PumpLogoLedAccessories;
        }

        public struct LedAccessory
        {
            public LedAccessoryType LedAccessoryType;
            public byte LedAccessoryIndex;
            public byte LedAccessoryChannel;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LedColour
        {
            public byte Green;
            public byte Red;
            public byte Blue;

            public static implicit operator LedColour(ColorSpace colour)
            {
                LedColour ledColour = new LedColour();
                if (colour != null)
                {
                    Rgb rgb = colour.To<Rgb>();
                    ledColour.Red = (byte)rgb.R;
                    ledColour.Blue = (byte)rgb.G;
                    ledColour.Green = (byte)rgb.B;
                }
                return ledColour;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SuperLedSetting
        {
            public byte Register;
            public byte Opcode;
            public byte Channel;
            private byte _padding0;
            public LedColour Colour00;      // :7
            public LedColour Colour01;      // :10
            public LedColour Colour02;      // :13
            public LedColour Colour03;      // :16
            public LedColour Colour04;      // :19
            public LedColour Colour05;      // :21
            public LedColour Colour06;      // :24
            public LedColour Colour07;      // :27
            public LedColour Colour08;      // :30
            public LedColour Colour09;      // :33
            public LedColour Colour10;      // :36
            public LedColour Colour11;      // :39
            public LedColour Colour12;      // :42
            public LedColour Colour13;      // :45
            public LedColour Colour14;      // :48
            public LedColour Colour15;      // :51
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ClassicLedSetting
        {
            public byte Opcode0;            // :0
            public byte Opcode1;            // :1
            public byte Address0;           // :2
            public byte Address1;           // :3
            public byte Mode;               // :4            
            public ushort Speed;            // :5            
            public LedColour Colour00;      // :7
            public LedColour Colour01;      // :10
            public LedColour Colour02;      // :13
            public LedColour Colour03;      // :16
            public LedColour Colour04;      // :19
            public LedColour Colour05;      // :21
            public LedColour Colour06;      // :24
            public LedColour Colour07;      // :27
            public LedColour Colour08;      // :30
            public LedColour Colour09;      // :33
            public LedColour Colour10;      // :36
            public LedColour Colour11;      // :39
            public LedColour Colour12;      // :42
            public LedColour Colour13;      // :45
            public LedColour Colour14;      // :48
            public LedColour Colour15;      // :51
            public byte Backwards;          // :55
            public byte ColourCount;        // :56
            public byte SpecialMode;        // :57
            public byte AvailableLeds;      // :58
            public byte AlternatingSize;    // :59
            private int _padding0;
        }
    }

}
