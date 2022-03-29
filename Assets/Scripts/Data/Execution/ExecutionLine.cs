using System;
using Unity.Entities;
using UnityEngine;

public enum ExecutionType
{
    // ------------------ Conventional -------------------
    GoTo,       // Goes to ExecutionLineIndex
    GoToTrue,   // Goes to ExecutionLineIndex if register true
    Exit,       // Exits Execution
    
    Set,        // Sets register to true;
    Copy,       // Copy register[1] from register[0];
    CmpLT,      // Sets register[2] to true if register[0]:Count is less than register[1]:Count

    // --------------------- Exotic -----------------------
    GoToRandom, // Go to random ExecutionLineIndex from buffer
    SkipAFrame, // Skips a frame

    // --------------------- Ant Behaviour ----------------
    AntMoveTo, // Moves Ant to Translation
    AntDestroy,// Destroys Ant
    AntPickResource, // Pick resource at ResourceStore, using RegisterIndex of bit-count 9 [Type|4 - Type|4 - Held|1]
    AntDropResource, // Drop resource at ResourceStore, using RegisterIndex of bit-count 9 [Type|4 - Type|4 - Held|1]
}

[InternalBufferCapacity(128)]
public struct ExecutionLine : IBufferElementData
{
    public ExecutionType type;
    public Entity ePtr; // Entity containing data relevant to do the type task. E.g. AntMoveTo. requires EntityBuffer present on this entity.

    ExecutionLine(ExecutionType type, Entity ePtr = new Entity())
    {
        this.type = type;
        this.ePtr = ePtr;
    }

    public static implicit operator ExecutionLine(ExecutionType type) => new ExecutionLine(type);
    public static implicit operator ExecutionLine((ExecutionType type, Entity e)t) => new ExecutionLine(t.type, t.e);
}

public struct ExecutionLineIndex : IComponentData
{
    short m_Line;
    public ExecutionLineIndex(short line) => m_Line = line;
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