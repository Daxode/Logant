using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class ColonyExecutionCreatorAuthor : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] Transform[] points;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ColonyExecutionCreatorTag());
        var buffer = dstManager.AddBuffer<EntityBufferElement>(entity);
        foreach (var point in points) 
            buffer.Add(conversionSystem.GetPrimaryEntity(point));
    }
}

public struct ColonyExecutionCreatorTag : IComponentData {}

[WorldSystemFilter(WorldSystemFilterFlags.Editor|WorldSystemFilterFlags.Default)]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
[AlwaysSynchronizeSystem]
public partial class SpawnExecutionSystem : SystemBase
{
    /// Entry 0: AntMoveTo, (Entry 1, Entry 2, Entry 3)
    /// Entry 1: AntMoveTo, (Entry 0)
    /// Entry 2: Stop
    /// Entry 3: Stop
    protected override void OnStartRunning()
    {
        var ColonyExecutionCreator = GetSingletonEntity<ColonyExecutionCreatorTag>();
        var points = GetBuffer<EntityBufferElement>(ColonyExecutionCreator);
        var HomeEntity = points[0];
        var FoodEntity = points[1];
        var ButtonEntity = points[2];
        var LakeEntity = points[3];

        // Entry 0
        var storage0 = EntityManager.CreateEntity(typeof(ColonyExecutionDataStorageTag));
        var storage0EntityBuffer = EntityManager.AddBuffer<EntityBufferElement>(storage0);
        storage0EntityBuffer.Add(LakeEntity);
        storage0EntityBuffer.Add(FoodEntity);
        storage0EntityBuffer.Add(ButtonEntity);
        
        // Entry 1
        var storage1 = EntityManager.CreateEntity(typeof(ColonyExecutionDataStorageTag));
        var storage1EntityBuffer = EntityManager.AddBuffer<EntityBufferElement>(storage1);
        storage1EntityBuffer.Add(HomeEntity);
        
        // Add to Execution Buffer
        var ColonyExecution = EntityManager.CreateEntity();
        EntityManager.SetName(ColonyExecution, "Colony Execution");
        var executionDataBuffer = EntityManager.AddBuffer<ColonyExecutionData>(ColonyExecution);
        executionDataBuffer.Add(new ColonyExecutionData(ColonyExecutionType.AntMoveTo, storage0));
        executionDataBuffer.Add(new ColonyExecutionData(ColonyExecutionType.AntMoveTo, storage1));
        executionDataBuffer.Add(new ColonyExecutionData(ColonyExecutionType.Stop));
        executionDataBuffer.Add(new ColonyExecutionData(ColonyExecutionType.Stop));
    }

    protected override void OnUpdate() {}
}