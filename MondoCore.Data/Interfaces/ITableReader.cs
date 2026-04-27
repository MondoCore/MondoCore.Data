/***************************************************************************
 *                                                                          
 *    The MondoCore Libraries  	                                            
 *                                                                          
 *      Namespace: MondoCore.Data	                                        
 *           File: ITableReader.cs                                                
 *      Class(es): ITableReader                                                   
 *        Purpose: Interface to read from a table
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
    /// Interface to read from a table
    /// </summary>
    public interface ITableReader<TID, TValue> where TValue : class, new()
    {
        Task<TValue>             Get(TID id, CancellationToken cancellationToken = default);
        Task<TValue>             Get(TID id, string? partitionKey, CancellationToken cancellationToken = default);
        IAsyncEnumerable<TValue> Get(IEnumerable<TID> ids, CancellationToken cancellationToken = default);
        IAsyncEnumerable<TValue> Get(Expression<Func<TValue, bool>> query, CancellationToken cancellationToken = default);
    }
}