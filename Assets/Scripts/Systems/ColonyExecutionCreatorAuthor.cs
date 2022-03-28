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
        var buffer = dstManager.AddBuffer<LocationEntityElement>(entity);
        foreach (var point in points) 
            buffer.Add(conversionSystem.GetPrimaryEntity(point));
    }
}

[InternalBufferCapacity(8)]
public struct LocationEntityElement : IBufferElementData
{
    public Entity m_E;
    public static implicit operator LocationEntityElement(Entity e) => new LocationEntityElement {m_E = e};
    public static implicit operator Entity(LocationEntityElement elem) => elem.m_E;
}

public struct ColonyExecutionCreatorTag : IComponentData {}

[WorldSystemFilter(WorldSystemFilterFlags.Editor|WorldSystemFilterFlags.Default)]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
[AlwaysSynchronizeSystem]
public partial class SpawnExecutionSystem : SystemBase
{
    /// Entry 0: AntMoveTo, (Entry 1, Entry 2, Entry 3)
    /// Entry 1: AntMoveTo, (Entry 0)
    /// Entry 2: Exit
    /// Entry 3: Exit
    protected override void OnStartRunning()
    {
        var ColonyExecutionCreator = GetSingletonEntity<ColonyExecutionCreatorTag>();
        var points = GetBuffer<LocationEntityElement>(ColonyExecutionCreator);
        Entity HomeEntity = points[0];
        Entity FoodEntity = points[1];
        Entity ButtonEntity = points[2];
        Entity LakeEntity = points[3];

        // Entry Home
        var homeStorage = EntityManager.CreateEntity();

        // Entry Lake
        var lakeStorage = EntityManager.CreateEntity();

        // Entry Button
        var buttonStorage = EntityManager.CreateEntity();

        // Entry Food
        var foodStorage = EntityManager.CreateEntity();


        // Add to Execution Buffer
        var ColonyExecution = EntityManager.CreateEntity();
        EntityManager.SetName(ColonyExecution, "Colony Execution");
        var executionDataBuffer = EntityManager.AddBuffer<ExecutionLine>(ColonyExecution);
        
    }

    protected override void OnUpdate() {}
}