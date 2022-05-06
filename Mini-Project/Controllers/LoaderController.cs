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
    public class LoaderController : ControllerBase
    {
        private LoaderService _LoaderService;

        public LoaderController(LoaderService loaderService)
        {
            _LoaderService = loaderService;
        }
        [HttpPost("post-to-database")]
        public IActionResult CopyToDatabase()
        {
            _LoaderService.CopyToDatabase();
            return Ok();
        }
    }
}
