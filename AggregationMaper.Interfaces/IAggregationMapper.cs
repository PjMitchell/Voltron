using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace AggregationMaper.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IAggregationMapper : IActor
    {
        Task<Dictionary<int, double>> MapAsync(IEnumerable<Dictionary<int, double>> values, CancellationToken cancellationToken);
    }
}
