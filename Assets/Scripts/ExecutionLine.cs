using Unity.Entities;

[GenerateAuthoringComponent]
public struct ExecutionLine : IComponentData
{
    public int line;
}