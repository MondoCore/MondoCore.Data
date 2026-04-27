
namespace MondoCore.Data
{
    public interface IPartitionedId
    {
        string  Id           { get; }
        string? PartitionKey { get; }
    }
}
