/***************************************************************************
 *                                                                          
 *    The MondoCore Libraries  	                                            
 *                                                                          
 *      Namespace: MondoCore.Data	                                        
 *           File: ITable.cs                                                
 *      Class(es): ITable                                                   
 *        Purpose: Interface to query tables                               
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

namespace MondoCore.Data
{
    /// <summary>
    /// Interface for a table
    /// </summary>
    public interface ITable<T> where T : class, new()
    {
        ITableReader<T> Reader { get; }
        ITableWriter<T> Writer { get; }
    }
}
