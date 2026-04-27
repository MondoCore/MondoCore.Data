
/***************************************************************************
 *                                                                          
 *    The MondoCore Libraries  	                                            
 *                                                                          
 *      Namespace: MondoCore.Data	                                        
 *           File: ITableWriter.cs                                                
 *      Class(es): ITableWriter                                                   
 *        Purpose: Interface to write to a table
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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MondoCore.Data
{
    /// <summary>
    /// Interface to write to a table
    /// </summary>
    public interface ITableWriter<TID, TValue> where TValue : class, new()
    {
        Task<TValue> Insert(TValue item, CancellationToken cancellationToken = default);
        Task         Insert(IEnumerable<TValue> items, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null);
        Task         Insert(IAsyncEnumerable<TValue> items, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null);
        
        Task<bool>   Update(TValue item, Expression<Func<TValue, bool>>? guard = null, CancellationToken cancellationToken = default);
        Task<long>   Update(object properties, Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null);
        Task<long>   Update(Func<TValue, Task<(bool Update, bool Continue)>> update, Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null);
        
        Task<bool>   Delete(TID id, CancellationToken cancellationToken = default);
        Task<long>   Delete(Expression<Func<TValue, bool>> guard, int maxItems = 0, CancellationToken cancellationToken = default, Func<Exception, Task>? onException = null);
    }
}
