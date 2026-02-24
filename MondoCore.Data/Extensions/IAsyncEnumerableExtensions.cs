/***************************************************************************
 *                                                                          
 *    The MondoCore Libraries  							                    
 *                                                                          
 *        Namespace: MondoCore.Data							            
 *             File: IAsyncEnumerableExtensions.cs					    		        
 *        Class(es): IAsyncEnumerableExtensions				         		            
 *          Purpose: Extensions for IAsyncEnumerable                  
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 16 Jan 2021                                             
 *                                                                          
 *   Copyright (c) 2021-2026 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MondoCore.Data
{
    /****************************************************************************/
    /****************************************************************************/
    public static class IAsyncEnumerableExtensions
    {
        /****************************************************************************/
        public static async Task ParallelForEach<T>(this IAsyncEnumerable<T> list, Func<long, T, Task> fnEach, int maxParallelism = 128, CancellationToken cancelToken = default)
        {
            maxParallelism = Math.Min(1024, Math.Max(2, maxParallelism));

            var block = new ActionBlock<Block<T>>(async (payload)=>
            {
                await fnEach(payload.Index, payload.Data);
            }, 
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxParallelism, CancellationToken = cancelToken });

            long i = 0L;

            await foreach(var item in list)
            {
                if(cancelToken.IsCancellationRequested)
                    break;

                await block.SendAsync(new Block<T> { Index = i++, Data = item } );
            }

            block.Complete();
            await block.Completion;
        }

        private class Block<T>
        {
            internal long Index { get; set; }
            internal T    Data  { get; set; }
        }
    }
}
