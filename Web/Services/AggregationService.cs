using AggregationMaper.Interfaces;
using AggregationReducer.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Web.Services
{
    public interface IAggregationService
    {
        Task<Dictionary<int, double>> Aggregate(IEnumerable<Dictionary<int, double>> source, CancellationToken cancellationToken);
    }

    public class AggregationService: IAggregationService
    {
        public async Task<Dictionary<int, double>> Aggregate(IEnumerable<Dictionary<int, double>> source, CancellationToken cancellationToken)
        {
            var reducer = ActorProxy.Create<IAggregationReducer>(ActorId.CreateRandom());
            var capacity = 4;
            var chunkSize = 50;
            var mapperPool = BuildMapperPool(capacity);
            var taskPool = new Task[capacity];
            var seedIndex = 0;
            foreach (var chunk in source.Chunk(chunkSize))
            {
                int index;
                if (seedIndex < capacity)
                {
                    index = seedIndex;
                    seedIndex++;
                }
                else
                {
                    var task = await Task.WhenAny(taskPool);
                    index = Array.IndexOf(taskPool, task);
                }

                var mapper = mapperPool[index];
                taskPool[index] = ProcessChunk(chunk, mapper, reducer, cancellationToken);
            }

            await Task.WhenAll(taskPool);
            return await reducer.GetResultAsync(cancellationToken);
        }

        private async Task ProcessChunk(IEnumerable<Dictionary<int, double>> source, IAggregationMapper mapper, IAggregationReducer reducer, CancellationToken cancellationToken)
        {
            var result = await mapper.MapAsync(source, cancellationToken);
            await reducer.ReduceAsync(result, cancellationToken);            
        }

        private IAggregationMapper[] BuildMapperPool(int capacity)
        {
            return Enumerable.Range(0, capacity).Select(s => ActorProxy.Create<IAggregationMapper>(ActorId.CreateRandom())).ToArray();
        }
    }

    public static class EnumerableEx
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int size)
        {
            var chunk = new List<T>(size);
            foreach(var item in source)
            {
                chunk.Add(item);
                if(chunk.Count == size)
                {
                    yield return chunk;
                    chunk = new List<T>();
                }
            }

            if(chunk.Count != 0)
                yield return chunk;
        }
    }
    
}
