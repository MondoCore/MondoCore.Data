
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
    public interface ITableWriter<T> where T : class, new()
    {
        Task<T>      Insert(T item, CancellationToken cancellationToken = default);
        Task         Insert(IEnumerable<T> items, CancellationToken cancellationToken = default);
        
        Task<bool>   Update(T item, Expression<Func<T, bool>> guard = null, CancellationToken cancellationToken = default);
        Task<long>   Update(object properties, Expression<Func<T, bool>> query, CancellationToken cancellationToken = default);
        Task<long>   Update(Func<T, Task<(bool Update, bool Continue)>> update, Expression<Func<T, bool>> query, CancellationToken cancellationToken = default);
        
        Task<bool>   Delete(string id, CancellationToken cancellationToken = default);
        Task<long>   Delete(Expression<Func<T, bool>> guard, CancellationToken cancellationToken = default);
    }
}
