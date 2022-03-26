using System;
using Unity.Entities;
using UnityEngine;

public enum ColonyExecutionType
{
    GoTo,
    Stop,
    AntMoveTo,
    AntWaitOn
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

[InternalBufferCapacity(8)]
public struct EntityBufferElement : IBufferElementData
{
    public Entity m_E;
    public static implicit operator EntityBufferElement(Entity e) => new EntityBufferElement {m_E = e};
    public static implicit operator Entity(EntityBufferElement elem) => elem.m_E;
}