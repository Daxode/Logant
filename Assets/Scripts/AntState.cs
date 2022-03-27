using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct AntState : IComponentData
{
    public uint id;
    public int executionLine;
    public AntFlags flags;
}

[Flags]
public enum AntFlags
{
    HasPickUp = 1,
    HasFood = 2,
}

/// Entry 0: AntMoveTo, (Entry 1, Entry 2, Entry 3)
/// Entry 1: AntMoveTo, (Entry 0)
/// Entry 2: Stop
/// Entry 3: Stop