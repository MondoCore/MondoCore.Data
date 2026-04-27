
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using MondoCore.Data.Tables;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MondoCore.Data.Memory.UnitTests")]

namespace MondoCore.Data.Memory
{
    /*********************************************************************/
    /*********************************************************************/
    public class MemoryTable<TID, TValue> : ITable<TID, TValue> where TValue : class, new()
    {
        private readonly TableDelegate<TID, TValue> _delegate = new MemoryTableDelegate<TID, TValue>();

        public ITableReader<TID, TValue> Reader => new TableReader<TID, TValue>(_delegate);
        public ITableWriter<TID, TValue> Writer => new TableWriter<TID, TValue>(_delegate);
    }

    /*********************************************************************/
    /*********************************************************************/
    internal class TableReader<TID, TValue> : ITableReader<TID, TValue> where TValue : class, new()
    {
        private readonly TableDelegate<TID, TValue> _delegate;

        /*********************************************************************/
        internal TableReader(TableDelegate<TID, TValue> tableDelegate)
        {
            _delegate = tableDelegate;
        }

        #region ITableReader

        /*********************************************************************/
        public Task<TValue> Get(TID id, CancellationToken cancellationToken = default)
        {
            return _delegate.Get(id, cancellationToken);
        }

        /*********************************************************************/
        public Task<TValue> Get(TID id, string? partitionKey, CancellationToken cancellationToken = default)
        {
            return _delegate.Get(id, partitionKey, cancellationToken);
        }

        /*********************************************************************/
        public IAsyncEnumerable<TValue> Get(IEnumerable<TID> ids, CancellationToken cancellationToken = default)
        {
            return _delegate.Get(ids, cancellationToken);
        }

        /*********************************************************************/
        public IAsyncEnumerable<TValue> Get(Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default)
        {
            return _delegate.Get(query, cancellationToken);
        }

        #endregion
    }

    /*********************************************************************/
    /*********************************************************************/
    internal class TableWriter<TID, TValue> : ITableWriter<TID, TValue> where TValue : class, new()
    {
        private readonly TableDelegate<TID, TValue> _delegate;

        /*********************************************************************/
        internal TableWriter(TableDelegate<TID, TValue> tableDelegate)
        {
            _delegate = tableDelegate;
        }

        #region ITableWriter

        /*********************************************************************/
        public Task<bool> Delete(TID id, CancellationToken cancellationToken = default)
        {
            return _delegate.Delete(id, cancellationToken);
        }

        /*********************************************************************/
        public Task<long> Delete(Expression<Func<TValue, bool>> guard, int maxItems = 0, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {
            return _delegate.Delete(guard, maxItems, cancellationToken, onException);
        }

        /*********************************************************************/
        public Task<TValue> Insert(TValue item, CancellationToken cancellationToken = default)
        {
            return _delegate.Insert(item, cancellationToken);
        }

        /*********************************************************************/
        public Task Insert(IEnumerable<TValue> items, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {
            return _delegate.Insert(items, cancellationToken, onException);         
        }

        /*********************************************************************/
        public Task Insert(IAsyncEnumerable<TValue> items, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {
            return _delegate.Insert(items, cancellationToken, onException);         
        }

        /*********************************************************************/
        public Task<bool> Update(TValue item, Expression<Func<TValue, bool>>? guard = null, CancellationToken cancellationToken = default)
        {
            return _delegate.Update(item, guard, cancellationToken);
        }

        /*********************************************************************/
        public Task<long> Update(object properties, Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {
            return _delegate.Update(properties, query, cancellationToken, onException);
        }

        /*********************************************************************/
        public Task<long> Update(Func<TValue, Task<(bool Update, bool Continue)>> update, Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null)
        {
            return _delegate.Update(update, query, cancellationToken, onException);
        }

        #endregion
    }
}
