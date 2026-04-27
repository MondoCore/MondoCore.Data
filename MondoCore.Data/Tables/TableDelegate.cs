/***************************************************************************
 *                                                                          
 *    The MondoCore Libraries  	                                            
 *                                                                          
 *      Namespace: MondoCore.Data	                                        
 *           File: TableDelegate.cs                                                
 *      Class(es): TableDelegate                                                   
 *        Purpose: Delegate for table operations
 *                                                                          
 *  Original Author: Jim Lightfoot                                         
 *    Creation Date: 23 Feb 2026                                           
 *                                                                          
 *   Copyright (c) 2026 - Jim Lightfoot, All rights reserved                
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using MondoCore.Common;

namespace MondoCore.Data.Tables
{
    /*********************************************************************/
    /*********************************************************************/
    public abstract class TableDelegate<TID, TValue> where TValue : class, new()
    {
        private readonly string? _partitionKeyField;

        protected TableDelegate(string? partitionKeyField)
        {
            _partitionKeyField = partitionKeyField;
        } 

        #region Read

        /*********************************************************************/
        public abstract Task<TValue> Get(TID id, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public abstract Task<TValue> Get(TID id, string? partitionKey, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public async IAsyncEnumerable<TValue> Get(IEnumerable<TID> ids, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach(var id in ids)
            { 
                if(cancellationToken.IsCancellationRequested)
                    yield break;

                yield return await Get(id, cancellationToken).ConfigureAwait(false);
            }
        }

        /*********************************************************************/
        public abstract IAsyncEnumerable<TValue> Get(Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default);

        #endregion

        #region Write

        /*********************************************************************/
        public abstract Task<TValue> Insert(TValue item, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public Task Insert(IEnumerable<TValue> items, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {        
             return Insert(items.ToAsyncEnumerable(), cancellationToken, onException);
        }

        /*********************************************************************/
        public async Task Insert(IAsyncEnumerable<TValue> list, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {        
            var count  = await ForEach
            (
                list, 
                int.MaxValue, 
                async (val, cancelToken)=>
                {
                    await this.Insert(val, cancellationToken: cancelToken).ConfigureAwait(false);

                    return true;
                },
                onException,
                cancellationToken
            ).ConfigureAwait(false);
        }

        /*********************************************************************/
        public abstract Task<bool> Delete(TID id, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public abstract Task<bool> Delete(TValue item, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public virtual async Task<long> Delete(Expression<Func<TValue, bool>> guard, int maxItems = 0, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {
            var result = Get(guard, cancellationToken);
            var count  = await ForEach
            (
                result, 
                maxItems, 
                async (val, cancelToken)=>
                {
                    await this.Delete(val, cancellationToken: cancelToken).ConfigureAwait(false);

                    return true;
                },
                onException,
                cancellationToken
            ).ConfigureAwait(false);

            return count;
        }

        /*********************************************************************/
        public async Task<bool> Update(TValue item, Expression<Func<TValue, bool>>? guard = null, CancellationToken cancellationToken = default)
        {
            var partitionKey = item.GetValue<string>(_partitionKeyField ?? "PartitionKey"); // ???
            var id = item.GetValue<TID>("Id")!;  // ???

            if (guard != null)
            { 
                var currentItem = await Get(id, partitionKey, cancellationToken).ConfigureAwait(false);
                var list        = (new List<TValue> {currentItem}) as IEnumerable<TValue>;
                var fnGuard     = guard.Compile();

                if(!list.Any(fnGuard))
                    return false;

                // Update partition key if it has changed
                partitionKey = currentItem.GetValue<string>(_partitionKeyField ?? "PartitionKey");
            }

            return await Update(item, partitionKey, cancellationToken).ConfigureAwait(false);
        }

        /*********************************************************************/
        protected abstract Task<bool> Update(TValue item, string? partitionKey, CancellationToken cancellationToken = default);

        /*********************************************************************/
        public virtual async Task<long> Update(object properties, Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {
            var result = Get(query, cancellationToken); 
            var count  = await ForEach
            (
                result, 
                int.MaxValue, 
                async (val, cancelToken)=>
                {
                    if(val.SetValues(properties))
                    { 
                        return await this.Update(val, cancellationToken: cancelToken).ConfigureAwait(false);
                    }

                    return false;
                },
                onException,
                cancellationToken
            ).ConfigureAwait(false);

            return count;
        }

        /*********************************************************************/
        public async Task<long> Update(Func<TValue, Task<(bool Update, bool Continue)>> fnUpdate, Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {
            var result = Get(query, cancellationToken: cancellationToken); 
            var count  = await ForEach
            (
                result, 
                int.MaxValue, 
                async (val, cancelToken)=>
                {
                    var result = await fnUpdate(val).ConfigureAwait(false);

                    if(result.Update)
                    { 
                        await this.Update(val, cancellationToken: cancelToken).ConfigureAwait(false);

                        return true;
                    }

                    return false;
                },
                onException,
                cancellationToken
            ).ConfigureAwait(false);

            return count;
        }

        #endregion

        #region Private

        /*********************************************************************/
        private async Task<long> ForEach(IAsyncEnumerable<TValue> items, int maxItems, Func<TValue, CancellationToken, Task<bool>>onEach, Func<Exception, Task>? onException, CancellationToken cancellationToken)
        {
            var count     = 0L;
            var cancelSrc = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            maxItems = maxItems > 0 ? maxItems : int.MaxValue;

            await items.ParallelForEach<TValue>(async (index, val)=>
            {
                try
                { 
                    if(Interlocked.Decrement(ref maxItems) < 0)
                    { 
                        cancelSrc.Cancel();
                        return;
                    }

                    if(await onEach(val, cancelSrc.Token).ConfigureAwait(false))
                        Interlocked.Increment(ref count);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }                
                catch(Exception ex)
                {
                    if(onException != null)
                        await onException(ex);
                }
            },
            cancelToken: cancelSrc.Token).ConfigureAwait(false);

            return count;
        }

        #endregion
    }
}
