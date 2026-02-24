
using MondoCore.Common;
using MondoCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MondoCore.Data.Tables
{
    /*********************************************************************/
    /*********************************************************************/
    public abstract class TableDelegate<T> where T : class, new()
    {
        private readonly string? _partitionKeyField;

        protected TableDelegate(string? partitionKeyField)
        {
            _partitionKeyField = partitionKeyField;
        } 

        #region Read

        /*********************************************************************/
        public abstract Task<T> Get(string id, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public abstract Task<T> Get(string id, string? partitionKey, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public async IAsyncEnumerable<T> Get(IEnumerable<string> ids, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach(var id in ids)
            { 
                if(cancellationToken.IsCancellationRequested)
                    yield break;

                yield return await Get(id, cancellationToken);
            }
        }

        /*********************************************************************/
        public abstract IAsyncEnumerable<T> Get(Expression<Func<T, bool>> query, CancellationToken cancellationToken = default);

        #endregion

        #region Write

        /*********************************************************************/
        public abstract Task<T> Insert(T item, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public Task Insert(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            Parallel.ForEach<T>(items, async (val)=>
            {
                if(cancellationToken.IsCancellationRequested)
                    return;

                await Insert(val, cancellationToken);
            });

            return Task.CompletedTask;
        }

        /*********************************************************************/
        public abstract Task<bool> Delete(string id, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public abstract Task<bool> Delete(T item, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public virtual async Task<long> Delete(Expression<Func<T, bool>> guard, CancellationToken cancellationToken = default)
        {
            var result = Get(guard, cancellationToken);
            var count  = 0L;

            await result.ParallelForEach<T>(async (index, val)=>
            {
                try
                { 
                    await this.Delete(val, cancellationToken: cancellationToken);

                    Interlocked.Increment(ref count);
                }
                catch
                {
                }
            },
            cancelToken: cancellationToken);

            return count;
        }

        /*********************************************************************/
        public async Task<bool> Update(T item, Expression<Func<T, bool>>? guard = null, CancellationToken cancellationToken = default)
        {
            var partitionKey = item.GetValue<string>(_partitionKeyField ?? "PartitionKey");
            var id = item.GetValue<string>("Id")!;

            if (guard != null)
            { 
                var currentItem = await Get(id, partitionKey, cancellationToken);
                var list        = (new List<T> {currentItem}) as IEnumerable<T>;
                var fnGuard     = guard.Compile();

                if(!list.Where(fnGuard).Any())
                    return false;

                // Update partition key if it has changed
                partitionKey = currentItem.GetValue<string>(_partitionKeyField ?? "PartitionKey");
            }

            return await Update(item, partitionKey, cancellationToken);
        }

        /*********************************************************************/
        protected abstract Task<bool> Update(T item, string? partitionKey, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public virtual async Task<long> Update(object properties, Expression<Func<T, bool>> query, CancellationToken cancellationToken = default)
        {
            var result = Get(query, cancellationToken: cancellationToken); 
            var count  = 0L;

            await result.ParallelForEach(async (index, val)=>
            {
                try
                { 
                    if(val.SetValues(properties))
                    { 
                        await this.Update(val, cancellationToken: cancellationToken);

                        Interlocked.Increment(ref count);
                    }
                }
                catch
                {
                }
            },
            cancelToken: cancellationToken);

            return count;
        }

        /*********************************************************************/
        public async Task<long> Update(Func<T, Task<(bool Update, bool Continue)>> fnUpdate, Expression<Func<T, bool>> query, CancellationToken cancellationToken = default)
        {
            var result = Get(query, cancellationToken: cancellationToken); 
            var count  = 0L;
            
            await result.ParallelForEach(async (index, val)=>
            {
                try
                { 
                    var result = await fnUpdate(val);

                    if(result.Update)
                    { 
                        await this.Update(val, cancellationToken: cancellationToken);

                        Interlocked.Increment(ref count);
                    }
                }
                catch
                {
                }
            },
            cancelToken: cancellationToken);

            return count;
        }

        #endregion
    }
}
