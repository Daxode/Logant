using System.Collections;
using System.Collections.Generic;
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
    int numberOfAnts;

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
    public int numberOfAnts;
    public int numberOfAntsSpawned;
    public Random random;
}

public partial class AnthillSpawnSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref AnthillData data, in Translation translation, in AntPrefab ant) =>
        {
            if (data.numberOfAnts > data.numberOfAntsSpawned)
            {
                var direction = data.random.NextFloat2Direction();
                var magnitude = data.random.NextFloat();
                var flatOffset = direction * magnitude*0.5f;

                var spawnedAnt = EntityManager.Instantiate(ant.prefab);
                SetComponent(spawnedAnt, new Translation { Value = translation.Value + new float3(flatOffset.x, 0, flatOffset.y)});
                SetComponent(spawnedAnt, new AntState { id = data.numberOfAntsSpawned });
                data.numberOfAntsSpawned++;
            }
        }).WithStructuralChanges().Run();
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