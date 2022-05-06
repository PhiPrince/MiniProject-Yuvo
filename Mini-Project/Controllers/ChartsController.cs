using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mini_Project.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mini_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private ChartsService _chartsService;
        public ChartsController(ChartsService chartsService)
        {
            _chartsService = chartsService;
        }
        [EnableCors("AllowOrigin")]
        [HttpGet("get-dates-per-hour")]
        public IActionResult getAggregatedData(string timeFrame,DateTime? dateFrom,DateTime? dateTo)
        {
            var values = _chartsService.getValues(timeFrame,dateFrom,dateTo);
            return Ok(values);
        }
    }
}
