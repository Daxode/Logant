using Unity.Entities;

[GenerateAuthoringComponent]
public struct FoodLeft : IComponentData
{
    public int Left;
    public int Total;
    public FoodLeft(int total, int left)
    {
        Total = total;
        Left = left;
    }
}
