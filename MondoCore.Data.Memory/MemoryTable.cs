
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
    public class MemoryTable<T> : ITable<T> where T : class, new()
    {
        private readonly TableDelegate<T> _delegate = new MemoryTableDelegate<T>();

        public ITableReader<T> Reader => new TableReader<T>(_delegate);
        public ITableWriter<T> Writer => new TableWriter<T>(_delegate);
    }

    /*********************************************************************/
    /*********************************************************************/
    internal class TableReader<T> : ITableReader<T> where T : class, new()
    {
        private readonly TableDelegate<T> _delegate;

        /*********************************************************************/
        internal TableReader(TableDelegate<T> tableDelegate)
        {
            _delegate = tableDelegate;
        }

        #region ITableReader

        /*********************************************************************/
        public Task<T> Get(string id, CancellationToken cancellationToken = default)
        {
            return _delegate.Get(id, cancellationToken);
        }

        /*********************************************************************/
        public Task<T> Get(string id, string? partitionKey, CancellationToken cancellationToken = default)
        {
            return _delegate.Get(id, partitionKey, cancellationToken);
        }

        /*********************************************************************/
        public IAsyncEnumerable<T> Get(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            return _delegate.Get(ids, cancellationToken);
        }

        /*********************************************************************/
        public IAsyncEnumerable<T> Get(Expression<Func<T, bool>> query, CancellationToken cancellationToken = default)
        {
            return _delegate.Get(query, cancellationToken);
        }

        #endregion
    }

    /*********************************************************************/
    /*********************************************************************/
    internal class TableWriter<T> : ITableWriter<T> where T : class, new()
    {
        private readonly TableDelegate<T> _delegate;

        /*********************************************************************/
        internal TableWriter(TableDelegate<T> tableDelegate)
        {
            _delegate = tableDelegate;
        }

        #region ITableWriter

        /*********************************************************************/
        public Task<bool> Delete(string id, CancellationToken cancellationToken = default)
        {
            return _delegate.Delete(id, cancellationToken);
        }

        /*********************************************************************/
        public Task<long> Delete(Expression<Func<T, bool>> guard, CancellationToken cancellationToken = default)
        {
            return _delegate.Delete(guard, cancellationToken);
        }

        /*********************************************************************/
        public Task<T> Insert(T item, CancellationToken cancellationToken = default)
        {
            return _delegate.Insert(item, cancellationToken);
        }

        /*********************************************************************/
        public Task Insert(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            return _delegate.Insert(items, cancellationToken);         
        }

        /*********************************************************************/
        public Task<bool> Update(T item, Expression<Func<T, bool>> guard = null, CancellationToken cancellationToken = default)
        {
            return _delegate.Update(item, guard, cancellationToken);
        }

        /*********************************************************************/
        public Task<long> Update(object properties, Expression<Func<T, bool>> query, CancellationToken cancellationToken = default)
        {
            return _delegate.Update(properties, query, cancellationToken);
        }

        /*********************************************************************/
        public Task<long> Update(Func<T, Task<(bool Update, bool Continue)>> update, Expression<Func<T, bool>> query, CancellationToken cancellationToken = default)
        {
            return _delegate.Update(update, query, cancellationToken);
        }

        #endregion
    }
}
