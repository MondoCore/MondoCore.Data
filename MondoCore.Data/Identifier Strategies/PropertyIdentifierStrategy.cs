using MondoCore.Common;

namespace MondoCore.Data
{
    /**************************************************************************/
    /// <summary>
    /// Retrieves the partition key from a property of the value object. The id is retrieved from the id property of the value object or from the id if it implements IPartitionedId
    /// </summary>
    public class PropertyIdentifierStrategy<TID, TVALUE> : IIdentifierStrategy<TID, TVALUE>
    {
        private readonly string _propertyName;

        public PropertyIdentifierStrategy(string propertyNam)
        {
            _propertyName = propertyNam;
        }

        /**************************************************************************/
        public (string Id, string PartitionKey) GetId(TID id, TVALUE val)
        {
            var partitionKey = val.GetValue<string>(_propertyName);
            var sid = SplitId(id);

            return (sid.Id, partitionKey);
        }

        /**************************************************************************/
        private (string Id, string? PartitionKey) SplitId(TID id)
        {
            var sid = "";
            string? partitionKey = null;

            if(id is IPartitionedId partitionedId)
            {
                sid = partitionedId.Id;
    
                if(!string.IsNullOrWhiteSpace(partitionedId.PartitionKey))
                { 
                    partitionKey = partitionedId.PartitionKey;

                    return (sid, partitionKey);
                }
            }

            return (id!.ToString()!, partitionKey);
        }     
    }
}
