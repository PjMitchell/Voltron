using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace AggregationReducer.Interfaces
{
    public interface IAggregationReducer : IActor
    {
        Task<Dictionary<int, double>> GetResultAsync(CancellationToken cancellationToken);
        Task ReduceAsync(Dictionary<int, double> subResult, CancellationToken cancellationToken);
    }
}
