using System;
using Unity.Entities;
using UnityEngine;

public enum ColonyExecutionType
{
    // ------------------ Conventional -------------------
    GoTo,       // Goes to ExecutionLine
    GoToTrue,   // Goes to ExecutionLine if register true
    Exit,       // Exits Execution
    Set,        // Sets value to true on register;
    Copy,       // Copy register 1 from register 0;


    // --------------------- Exotic -----------------------
    GoToRandom, // Go to random ExecutionLine from buffer

    // --------------------- Ant Behaviour ----------------
    AntMoveTo, // Moves Ant to Translation
}

[InternalBufferCapacity(128)]
public struct ColonyExecutionData : IBufferElementData
{
    public ColonyExecutionType type;
    public Entity storageEntity; // Entity containing data relevant to do the type task. E.g. AntMoveTo. requires EntityBuffer present on this entity.
}