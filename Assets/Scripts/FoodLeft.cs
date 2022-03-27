using Unity.Entities;

[GenerateAuthoringComponent]
public struct FoodLeft : IComponentData
{
    public int Left;
    public int Total;
}
