using Unity.Entities;

[GenerateAuthoringComponent]
public struct ExecutionLine : IComponentData
{
    short m_Line;
    public static implicit operator ExecutionLine(short s) => new ExecutionLine {m_Line = s};
    public static implicit operator short(ExecutionLine executionLine) => executionLine.m_Line;
}

[InternalBufferCapacity(8)]
public struct ExecutionLineElement : IBufferElementData
{
    short m_Line;
    public static implicit operator ExecutionLineElement(short s) => new ExecutionLineElement {m_Line = s};
    public static implicit operator short(ExecutionLineElement executionLine) => executionLine.m_Line;
}