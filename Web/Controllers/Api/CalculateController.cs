using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Web.Services;

namespace Web.Controllers.Api
{
    [Route("api/Calculate")]
    public class CalculateController : Controller
    {
        private readonly IAggregationService aggregated;

        public CalculateController(IAggregationService aggregated)
        {
            this.aggregated = aggregated;
        }

        [HttpGet]
        public async Task<Dictionary<int, double>> Get()
        {
            var input = Enumerable.Range(0, 200000).Select(s => Build());
            var sw = Stopwatch.StartNew();
            var result = await aggregated.Aggregate(input, CancellationToken.None);
            sw.Stop();
            return result;
        }

        private Dictionary<int, double> Build()
        {
            return new Dictionary<int, double>
            {
                [20] = 2.3,
                [21] = 3.3,
                [22] = 2.3,
                [23] = 21.3,
                [25] = 2.3,
                [29] = 2.4,
                [27] = 2.1,

            };
        }
    }
}
