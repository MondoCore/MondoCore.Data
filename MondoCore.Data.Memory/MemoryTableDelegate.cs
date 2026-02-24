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
    internal class MemoryTableDelegate<T> : TableDelegate<T> where T : class, new()
    {
        private readonly ConcurrentDictionary<string, T> _items = new ConcurrentDictionary<string, T>();

        internal MemoryTableDelegate() : base("__bob")
        {
        }

        #region Read

        /*********************************************************************/
        public override Task<T> Get(string id, CancellationToken cancellationToken = default)
        {
            if(!_items.ContainsKey(id))
                throw new NotFoundException();

            return Task.FromResult(_items[id]);
        }

        /*********************************************************************/
        public override Task<T> Get(string id, string? partitionKey, CancellationToken cancellationToken = default)
        {
            if(!_items.ContainsKey(id))
                throw new NotFoundException();

            return Task.FromResult(_items[id]);
        }

        /*********************************************************************/
        public override async IAsyncEnumerable<T> Get(Expression<Func<T, bool>> query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        public override Task<T> Insert(T item, CancellationToken cancellationToken = default)
        {
            _items[item.GetValue<string>("Id")!] = item;

            return Task.FromResult(item);
        }

        /*********************************************************************/
        public override Task<bool> Delete(string id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.TryRemove(id, out _));
        }

        /*********************************************************************/
        public override Task<bool> Delete(T item, CancellationToken cancellationToken = default)
        {
            return Delete(item.GetValue<string>("Id")!, cancellationToken);
        }

        /*********************************************************************/
        protected override Task<bool> Update(T item, string? partitionKey, CancellationToken cancellationToken = default)
        {
            var id = item.GetValue<string>("Id")!;

            if(!_items.ContainsKey(id))
                throw new NotFoundException();

            _items[id] = item;

            return Task.FromResult(true);
        }

        #endregion
    }
}
