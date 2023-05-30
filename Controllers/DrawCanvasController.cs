using Microsoft.AspNetCore.Mvc;

namespace WEB_API_ASP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CanvasController : ControllerBase
    {
        private static readonly string[] Summaries = { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        private readonly ILogger<CanvasController> _logger;

        public CanvasController(ILogger<CanvasController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetDrawCanvas")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        [Route("drawcanvas")]
        public string[] PostCanvas()
        {
            string[] strings = { "a", "b" };
            return strings;
        }
    }
}