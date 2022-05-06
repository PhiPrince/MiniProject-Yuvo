using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mini_Project.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregatorController : ControllerBase
    {
        private AggregatorService _aggregatorService;

        public AggregatorController(AggregatorService aggregatorService)
        {
            _aggregatorService = aggregatorService;
        }
        [HttpPost("aggregate")]
        public IActionResult AggregateData()
        {
            _aggregatorService.AggregateData();
            return Ok();
        }
    }
}
