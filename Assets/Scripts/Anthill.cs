using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

public class Anthill : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [SerializeField]
    GameObject antPrefab;
    [SerializeField]
    uint numberOfAnts;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new AntPrefab { prefab = conversionSystem.GetPrimaryEntity(antPrefab)});
        dstManager.AddComponentData(entity, new AnthillData { numberOfAnts = numberOfAnts, random = new Random(100) });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(antPrefab);
    }
}

public struct AntPrefab : IComponentData
{
    public Entity prefab;
}

public struct AnthillData : IComponentData
{
    public uint numberOfAnts;
    public uint numberOfAntsSpawned;
    public Random random;
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class AnthillSpawnSystem : SystemBase
{
    // float SpawnInterval = 0.05f;
    // float SpawnTimeLeft;
    protected override void OnUpdate()
    {
        // if (SpawnTimeLeft < 0)
        // {
            Entities.ForEach((ref AnthillData data, in Translation translation, in AntPrefab ant) =>
            {
                const int BatchCount = 10;
                if (data.numberOfAnts > data.numberOfAntsSpawned+BatchCount)
                {
                    var spawnedAnts = EntityManager.Instantiate(ant.prefab, BatchCount, Allocator.Temp);
                    for (uint i = 0; i < BatchCount; i++)
                    {
                        var direction = data.random.NextFloat2Direction();
                        var magnitude = data.random.NextFloat();
                        var flatOffset = direction * magnitude * 0.5f;
                        SetComponent(spawnedAnts[(int) i], new Translation {Value = translation.Value + new float3(flatOffset.x, 0, flatOffset.y)});
                        SetComponent(spawnedAnts[(int) i], new AntState {id = data.numberOfAntsSpawned+i});
                    }
                    data.numberOfAntsSpawned+=BatchCount;
                }
            }).WithStructuralChanges().Run();
        //     SpawnTimeLeft = SpawnInterval;
        // }else SpawnTimeLeft -= Time.DeltaTime;
    }
}

public partial class PhysicsLockingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<InertiaNotLocked>().ForEach((Entity e,ref PhysicsMass mass) =>
        {
            mass.InverseInertia = math.up();
            EntityManager.RemoveComponent<InertiaNotLocked>(e);
        }).WithStructuralChanges().Run();
    }
}