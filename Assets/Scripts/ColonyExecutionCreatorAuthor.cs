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
    /// Entry 2: Stop
    /// Entry 3: Stop
    protected override void OnStartRunning()
    {
        var ColonyExecutionCreator = GetSingletonEntity<ColonyExecutionCreatorTag>();
        var points = GetBuffer<LocationEntityElement>(ColonyExecutionCreator);
        Entity HomeEntity = points[0];
        Entity FoodEntity = points[1];
        Entity ButtonEntity = points[2];
        Entity LakeEntity = points[3];

        // Entry Home
        var homeStorage = EntityManager.CreateEntity(typeof(ColonyExecutionDataStorageTag));
        var storage0PickBuffer = EntityManager.AddBuffer<PickUpEntityElement>(homeStorage);
        storage0PickBuffer.Add(LakeEntity);
        storage0PickBuffer.Add(ButtonEntity);
        
        // Entry Lake
        var lakeStorage = EntityManager.CreateEntity(typeof(ColonyExecutionDataStorageTag));
        var lakeStoragePickBuffer = EntityManager.AddBuffer<PickUpEntityElement>(lakeStorage);
        lakeStoragePickBuffer.Add(FoodEntity);
        lakeStoragePickBuffer.Add(ButtonEntity);
        var lakeStorageDropBuffer = EntityManager.AddBuffer<DropDownEntityElement>(lakeStorage);
        lakeStorageDropBuffer.Add(HomeEntity);
        
        // Entry Button
        var buttonStorage = EntityManager.CreateEntity(typeof(ColonyExecutionDataStorageTag));
        var buttonStoragePickBuffer = EntityManager.AddBuffer<PickUpEntityElement>(buttonStorage);
        buttonStoragePickBuffer.Add(FoodEntity);
        var buttonStorageDropBuffer = EntityManager.AddBuffer<DropDownEntityElement>(buttonStorage);
        buttonStorageDropBuffer.Add(HomeEntity);
        buttonStorageDropBuffer.Add(LakeEntity);

        // Entry Food
        var foodStorage = EntityManager.CreateEntity(typeof(ColonyExecutionDataStorageTag));
        var foodStoragePickBuffer = EntityManager.AddBuffer<PickUpEntityElement>(foodStorage);
        foodStoragePickBuffer.Add(HomeEntity);
        var foodStorageDropBuffer = EntityManager.AddBuffer<DropDownEntityElement>(foodStorage);
        foodStorageDropBuffer.Add(ButtonEntity);

        // Add to Execution Buffer
        var ColonyExecution = EntityManager.CreateEntity();
        EntityManager.SetName(ColonyExecution, "Colony Execution");
        var executionDataBuffer = EntityManager.AddBuffer<ColonyExecutionData>(ColonyExecution);
        SetComponent(HomeEntity, new ExecutionLine{line = 0});
        executionDataBuffer.Add(new ColonyExecutionData(ColonyExecutionType.AntMoveTo, homeStorage));
        SetComponent(LakeEntity, new ExecutionLine{line = 1});
        executionDataBuffer.Add(new ColonyExecutionData(ColonyExecutionType.AntMoveTo, lakeStorage));
        SetComponent(ButtonEntity, new ExecutionLine{line = 2});
        executionDataBuffer.Add(new ColonyExecutionData(ColonyExecutionType.AntMoveTo, buttonStorage));
        SetComponent(FoodEntity, new ExecutionLine{line = 3});
        executionDataBuffer.Add(new ColonyExecutionData(ColonyExecutionType.AntMoveTo, foodStorage));
    }

    protected override void OnUpdate() {}
}