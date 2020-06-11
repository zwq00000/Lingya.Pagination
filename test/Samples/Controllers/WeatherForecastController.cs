using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lingya.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Samples.Controllers {
    [ApiController]
    [Route ("[controller]")]
    public class WeatherForecastController : ControllerBase {
        private static readonly string[] Summaries = new [] {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly AppDbContext context;

        public WeatherForecastController (AppDbContext context, ILogger<WeatherForecastController> logger) {
            _logger = logger;
            this.context = context;
        }

        [HttpGet]
        public PageResult<WeatherForecast> GetForPage ([FromQuery] PageParameter parameter) {
            var rng = new Random ();
            return Enumerable.Range (1, 100).Select (index => new WeatherForecast {
                    Date = DateTime.Now.AddDays (index),
                        TemperatureC = rng.Next (-20, 55),
                        Summary = Summaries[rng.Next (Summaries.Length)]
                }).AsQueryable ().PagingBuilder (parameter).StartsFor (w => w.Summary)
                .ToPaging ();;
        }

        [HttpGet ("async")]
        public async Task<PageResult<WeatherForecast>> GetForPageAsync ([FromQuery] PageParameter parameter) {
            return await context.WeatherForecasts
                .PagingBuilder (parameter)
                .StartsFor (w => w.Summary)
                .ToPagingAsync ();;
        }

        [HttpGet ("{id}")]
        public async Task<WeatherForecast> GetById (int id) {
            return await context.WeatherForecasts.FindAsync (id);
        }

        [HttpPost ()]
        public async Task<ActionResult> Create ([FromForm] WeatherForecast weather) {
            if (this.ModelState.IsValid) {
                //weather.Id = 0;
                await context.WeatherForecasts.AddAsync (weather);
                await context.SaveChangesAsync ();
                return AcceptedAtAction (nameof (GetById), new { id = weather.Id }, weather);
            }
            return Ok (ModelState);
        }

        [HttpPut ("{id}")]
        public async Task<ActionResult> Update (int id,WeatherForecast weather) {
            if (this.ModelState.IsValid) {
                if(id!= weather.Id){
                    return BadRequest();
                }
                // var entity = await context.WeatherForecasts.FindAsync(id);
                // if(entity == null){
                //     return NotFound();
                // }
                //context.Entry(entity).
                //weather.Id = 0;
                context.WeatherForecasts.Update(weather);
                await context.SaveChangesAsync ();
                return AcceptedAtAction (nameof (GetById), new { id = weather.Id }, weather);
            }
            return Ok (ModelState);
        }

        /// <summary>
        /// generate demo data
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpPost ("Generate")]
        public async Task GenerateDataAsync (int count = 100) {
            var rng = new Random ();
            var values = Enumerable.Range (1, count).Select (index => new WeatherForecast {
                Date = DateTime.Now.AddDays (index),
                    TemperatureC = rng.Next (-20, 55),
                    Summary = Summaries[rng.Next (Summaries.Length)]
            });

            await context.WeatherForecasts.AddRangeAsync (values);
            await context.SaveChangesAsync ();
        }
    }
}