using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NeoAcheron.SystemMonitor.Core.Config;
using NeoAcheron.SystemMonitor.Core.Controllers;
using NeoAcheron.SystemMonitor.Web.Utils;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NeoAcheron.SystemMonitor.Web.Controllers
{
    [Route("api/[controller]")]
    public class ControlController : Controller
    {
        private Dictionary<string, IAdjuster> adjusters = new Dictionary<string, IAdjuster>();
        private readonly Core.SystemTraverser systemTraverser;
        private readonly AdjusterConfig adjusterConfig;

        public ControlController(Core.SystemTraverser systemTraverser, AdjusterConfig adjusterConfig)
        {
            this.systemTraverser = systemTraverser;
            this.adjusterConfig = adjusterConfig;

            foreach (IAdjuster adjuster in adjusterConfig.Adjusters)
            {
                if (adjuster.ControlledSettingPaths == null) continue;
                foreach (string controlPath in adjuster.ControlledSettingPaths)
                {
                    if (controlPath == null) continue;
                    if (!adjusters.ContainsKey(controlPath))
                    {
                        adjusters.Add(controlPath, adjuster);
                    }
                }
            }
        }

        [HttpGet]
        public dynamic Get()
        {
            return adjusters;
        }

        [HttpGet("{**controlPath}")]
        public IActionResult Get([FromRoute]string controlPath)
        {
            if (!systemTraverser.AllSettings.Any(s => s.Path == controlPath)) return NotFound("Control path not found");
            if (controlPath == "42")
            {
                return Ok(new LinearAdjuster());
            }
            if (adjusters.ContainsKey(controlPath))
            {
                return Ok(adjusters[controlPath]);
            }
            else
            {
                return Ok(new DefaultAdjuster()
                {
                    SettingPath = controlPath
                });
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{**controlPath}")]
        public IActionResult Put([FromRoute]string controlPath, [FromBody]IAdjuster adjuster)
        {
            if (!systemTraverser.AllSettings.Any(s => s.Path == controlPath)) return NotFound("Control path not found");

            if (adjusters.ContainsKey(controlPath))
            {
                adjusters[controlPath].Stop();
            }

            bool success = adjuster.Start(systemTraverser);
            if (success)
            {
                adjusters[controlPath] = adjuster;
            }
            else
            {
                if (adjusters.ContainsKey(controlPath))
                    adjusters[controlPath].Start(systemTraverser);
                return BadRequest("The control could not be started");
            }

            adjusterConfig.Adjusters = adjusters.Values.ToList();
            adjusterConfig.Save();
            return Ok();
        }

        // DELETE api/<controller>/5
        [HttpDelete("{**controlPath}")]
        public IActionResult Delete([FromRoute]string controlPath)
        {
            if (!systemTraverser.AllSettings.Any(s => s.Path == controlPath)) return NotFound("Control path not found");

            if (adjusters.ContainsKey(controlPath))
            {
                bool success = adjusters[controlPath].Stop();
                if (success)
                {
                    adjusters.Remove(controlPath);
                    adjusterConfig.Adjusters = adjusters.Values.ToList();
                    adjusterConfig.Save();
                }
            }
            return Ok();
        }
    }
}
