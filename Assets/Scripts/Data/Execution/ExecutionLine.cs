using System;
using Unity.Entities;
using UnityEngine;

public enum ColonyExecutionType
{
    // ------------------ Conventional -------------------
    GoTo,       // Goes to ExecutionLineIndex
    GoToTrue,   // Goes to ExecutionLineIndex if register true
    Exit,       // Exits Execution
    Set,        // Sets value to true on register;
    Copy,       // Copy register 1 from register 0;


    // --------------------- Exotic -----------------------
    GoToRandom, // Go to random ExecutionLineIndex from buffer

    // --------------------- Ant Behaviour ----------------
    AntMoveTo, // Moves Ant to Translation
}

[InternalBufferCapacity(128)]
public struct ExecutionLine : IBufferElementData
{
    public ColonyExecutionType type;
    public Entity storageEntity; // Entity containing data relevant to do the type task. E.g. AntMoveTo. requires EntityBuffer present on this entity.
}

[GenerateAuthoringComponent]
public struct ExecutionLineIndex : IComponentData
{
    short m_Line;
    public static implicit operator ExecutionLineIndex(short s) => new ExecutionLineIndex {m_Line = s};
    public static implicit operator short(ExecutionLineIndex executionLineIndex) => executionLineIndex.m_Line;
}

[InternalBufferCapacity(8)]
public struct ExecutionLineIndexElement : IBufferElementData
{
    short m_Line;
    public static implicit operator ExecutionLineIndexElement(short s) => new ExecutionLineIndexElement {m_Line = s};
    public static implicit operator short(ExecutionLineIndexElement executionLineIndex) => executionLineIndex.m_Line;
}