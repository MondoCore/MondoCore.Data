
namespace MondoCore.Data
{
    public interface IIdentifierStrategy<TID>
    {
       (string Id, string PartitionKey) GetId(TID id);
    }

    public interface IIdentifierStrategy<TID, TVALUE> : IIdentifierStrategy<TID>
    {
       (string Id, string PartitionKey) GetId(TID id, TVALUE val);
    }
}
