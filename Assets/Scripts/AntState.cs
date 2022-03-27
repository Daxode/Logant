using System;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct AntState : IComponentData
{
    public short id;
    public byte executionLine;
}

[GenerateAuthoringComponent]
public struct AntStack : IComponentData
{
    long m_Stack;
    short m_StackPtr;
    public AntStack(long stack, short stackPtr)
    {
        m_Stack = stack;
        m_StackPtr = stackPtr;
    }

    public bool Pop()
    {
        var bit = 1L << m_StackPtr;
        m_StackPtr--;
        return bit == (m_Stack&bit);
    }
    
    public bool Peek()
    {
        var bit = 1L << m_StackPtr;
        return bit == (m_Stack&bit);
    }
    
    public void Push(bool val)
    {
        m_Stack |= 1L << m_StackPtr;
        m_StackPtr++;
    }
}