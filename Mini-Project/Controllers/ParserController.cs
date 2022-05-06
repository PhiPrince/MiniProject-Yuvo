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
    public class ParserController : ControllerBase
    {
        private ParserService _ParserService;
        public ParserController(ParserService parserService)
        {
            _ParserService = parserService;
        }
        [HttpPost("start-parser")]
        public IActionResult StartParser()
        {
            _ParserService.ParseFiles();
            return Ok();
        }
    }
}
