
using Microsoft.AspNetCore.Mvc;
using NginxServerFarms.Services;
using System.Collections.Generic;

namespace NginxServerFarms.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class NginxController : ControllerBase {
        private readonly INginxConfigFileService configFileService;

        public NginxController(
            INginxConfigFileService configFileService) {
            this.configFileService = configFileService;
        }

        [HttpGet("upstreams")]
        public IReadOnlyList<NginxUpstream> Get() {
            return this.configFileService.ReadUpstreams();
        }

        [HttpPost("upstreams")]
        public void Post(IReadOnlyList<NginxUpstream> upstreams) {
            this.configFileService.WriteUpstreams(upstreams);
        }
    }
}