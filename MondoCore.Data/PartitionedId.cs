
namespace MondoCore.Data
{
    public class PartitionedId : IPartitionedId
    {
        public string  Id           { get; set; } = default!;
        public string? PartitionKey { get; set; }
    }
}
