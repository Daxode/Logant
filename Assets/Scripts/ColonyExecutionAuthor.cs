using System;
using Unity.Entities;
using UnityEngine;

public enum ColonyExecutionType
{
    // Conventional
    GoTo,
    Stop,
    Push,
    
    
    // Exotic
    GoToOnTrue,

    // Ant Behaviour
    AntMoveTo,
}

[InternalBufferCapacity(32)]
public struct ColonyExecutionData : IBufferElementData
{
    public ColonyExecutionType type;
    public Entity storageEntity; // Entity containing data relevant to do the type task. E.g. AntMoveTo. requires EntityBuffer present on this entity.

    public ColonyExecutionData(ColonyExecutionType type, Entity storageEntity = new Entity())
    {
        this.type = type;
        this.storageEntity = storageEntity;
    }
}

public struct ColonyExecutionDataStorageTag : IComponentData {}

// Used by MoveToAnt.

[InternalBufferCapacity(8)]
public struct PickUpEntityElement : IBufferElementData
{
    public Entity m_E;
    public static implicit operator PickUpEntityElement(Entity e) => new PickUpEntityElement {m_E = e};
    public static implicit operator Entity(PickUpEntityElement elem) => elem.m_E;
}

[InternalBufferCapacity(8)]
public struct DropDownEntityElement : IBufferElementData
{
    public Entity m_E;
    public static implicit operator DropDownEntityElement(Entity e) => new DropDownEntityElement {m_E = e};
    public static implicit operator Entity(DropDownEntityElement elem) => elem.m_E;
}