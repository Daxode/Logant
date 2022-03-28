using Unity.Entities;

[GenerateAuthoringComponent]
public struct FoodStore : IComponentData
{
    public int Left;
    public int Total;
    public FoodStore(int total, int left)
    {
        Total = total;
        Left = left;
    }
}
