using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeoAcheron.SystemMonitor.Core;
using NeoAcheron.SystemMonitor.Core.Config;
using NeoAcheron.SystemMonitor.Web.Models;
using NeoAcheron.SystemMonitor.Web.Utils;

namespace NeoAcheron.SystemMonitor.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HardwareController : ControllerBase
    {
        private readonly SystemTraverser traverser;
        private readonly SensorConfig sensorConfig;

        private class SystemTreeBuilder : IVisitor
        {
            private readonly SensorConfig sensorConfig;
            public HardwareDescriptor entry;

            public SystemTreeBuilder(SensorConfig sensorConfig)
            {
                this.sensorConfig = sensorConfig;
            }

            public void VisitComputer(IComputer computer)
            {
                var instance = new HardwareDescriptor();
                instance.Name = "Computer";
                instance.Type = "root";
                instance.Path = "";

                entry = instance;

                var temp = entry;
                computer.Traverse(this);
                entry = temp;
            }

            public void VisitHardware(IHardware hardware)
            {
                var instance = new HardwareDescriptor();
                instance.Type = hardware.HardwareType.ToString();
                instance.Path = hardware.Identifier.ToString().Trim('/');
                instance.Name = sensorConfig.GetName(instance.Path, hardware.Name);

                entry.Children.Add(instance);

                var temp = entry;

                entry = instance;
                hardware.Traverse(this);
                entry = temp;
            }

            public void VisitParameter(IParameter parameter)
            {
                parameter.Traverse(this);
            }

            public void VisitSensor(ISensor sensor)
            {
                var instance = new SensorDescriptor();
                instance.Type = sensor.SensorType.ToString().ToLower();
                instance.Path = sensor.Identifier.ToString().Trim('/');
                instance.Name = sensorConfig.GetName(instance.Path, sensor.Name);
                instance.Hidden = sensorConfig.IsHidden(instance.Path, false);
                instance.Primary = sensorConfig.IsPrimary(instance.Path, false);

                List<SensorDescriptor> descriptors = entry.Sensors.GetValueOrDefault(instance.Type, new List<SensorDescriptor>());
                descriptors.Add(instance);

                if (!entry.Sensors.ContainsKey(instance.Type))
                {
                    entry.Sensors.Add(instance.Type, descriptors);
                }

                if (sensor.Control != null)
                {
                    instance.ControlPath = sensor.Control.Identifier.ToString().Trim('/');
                }
            }
        }

        public HardwareController(SystemTraverser traverser, SensorConfig sensorConfig)
        {
            this.traverser = traverser;
            this.sensorConfig = sensorConfig;
        }

        [HttpGet]
        public IList<HardwareDescriptor> Get()
        {
            var systemTreeBuilder = new SystemTreeBuilder(sensorConfig);
            traverser.computer.Accept(systemTreeBuilder);
            return systemTreeBuilder.entry.Children;
        }

        [HttpPut]
        public SensorDescriptor Put([FromBody]SensorDescriptor sensorDescriptor)
        {
            sensorConfig.AddOrUpdatePrimary(sensorDescriptor.Path, sensorDescriptor.Primary);
            sensorConfig.AddOrUpdateName(sensorDescriptor.Path, sensorDescriptor.Name);
            sensorConfig.AddOrUpdateHidden(sensorDescriptor.Path, sensorDescriptor.Hidden);
            sensorConfig.Save();

            return sensorDescriptor;
        }
    }
}
