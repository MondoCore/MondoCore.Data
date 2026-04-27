using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using MondoCore.Common;
using MondoCore.Data.Tables;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MondoCore.Data.Memory.UnitTests")]

namespace MondoCore.Data.Memory
{
    /*********************************************************************/
    /*********************************************************************/
    internal class MemoryTableDelegate<TID, TValue> : TableDelegate<TID, TValue> where TValue : class, new()
    {
        private readonly ConcurrentDictionary<TID, TValue> _items = new ConcurrentDictionary<TID, TValue>();

        internal MemoryTableDelegate() : base("__bob")
        {
        }

        #region Read

        /*********************************************************************/
        public override Task<TValue> Get(TID id, CancellationToken cancellationToken = default)
        {
            if(!_items.ContainsKey(id))
                throw new NotFoundException();

            return Task.FromResult(_items[id]);
        }

        /*********************************************************************/
        public override Task<TValue> Get(TID id, string? partitionKey, CancellationToken cancellationToken = default)
        {
            if(!_items.ContainsKey(id))
                throw new NotFoundException();

            return Task.FromResult(_items[id]);
        }

        /*********************************************************************/
        public override async IAsyncEnumerable<TValue> Get(Expression<Func<TValue, bool>> query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var fnQuery = query.Compile();

            foreach(var item in _items.Values)
            {
                if(cancellationToken.IsCancellationRequested)
                    yield break;

                if(fnQuery(item))
                    yield return await Task.FromResult(item);
            }   
        }

        #endregion

        #region Write

        /*********************************************************************/
        public override Task<TValue> Insert(TValue item, CancellationToken cancellationToken = default)
        {
            _items[item.GetValue<TID>("Id")!] = item;

            return Task.FromResult(item);
        }

        /*********************************************************************/
        public override Task<bool> Delete(TID id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.TryRemove(id, out _));
        }

        /*********************************************************************/
        public override Task<bool> Delete(TValue item, CancellationToken cancellationToken = default)
        {
            return Delete(item.GetValue<TID>("Id")!, cancellationToken);
        }

        /*********************************************************************/
        protected override Task<bool> Update(TValue item, string? partitionKey, CancellationToken cancellationToken = default)
        {
            var id = item.GetValue<TID>("Id")!;

            if(!_items.ContainsKey(id))
                throw new NotFoundException();

            _items[id] = item;

            return Task.FromResult(true);
        }

        #endregion
    }
}
