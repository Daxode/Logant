using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceStoreAuthour : IComponentData
{
    public ResourceType Type;
    public uint Left;
    public uint Total;
    public ResourceStoreAuthour(ResourceType type, uint left, uint total)
    {
        Type = type;
        Left = left;
        Total = total;
    }
}

public enum ResourceType {
    Food,
    Key
}
