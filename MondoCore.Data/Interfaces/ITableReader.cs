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
    public interface ITableReader<T> where T : class, new()
    {
        Task<T>             Get(string id, CancellationToken cancellationToken = default);
        Task<T>             Get(string id, string? partitionKey, CancellationToken cancellationToken = default);
        IAsyncEnumerable<T> Get(IEnumerable<string> ids, CancellationToken cancellationToken = default);
        IAsyncEnumerable<T> Get(Expression<Func<T, bool>> query, CancellationToken cancellationToken = default);
    }
}