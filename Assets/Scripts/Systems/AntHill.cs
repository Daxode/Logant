using System.Collections.Generic;
using Data;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

namespace Systems
{
    public class AntHill : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [SerializeField] GameObject antPrefab;
        [SerializeField] short numberOfAnts;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AntPrefab {prefab = conversionSystem.GetPrimaryEntity(antPrefab)});
            dstManager.AddComponentData(entity, new AntHillData {numberOfAnts = numberOfAnts, random = new Random(100)});
        }
        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) => referencedPrefabs.Add(antPrefab);
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class AntHillSpawnSystem : SystemBase
    {
        float m_SpawnTimeLeft;
        protected override void OnUpdate()
        {
            if (m_SpawnTimeLeft < 0)
            {
                Entities.ForEach((ref AntHillData data, in Translation translation, in AntPrefab ant) =>
                {
                    const short batchCount = 1;
                    if (data.numberOfAnts >= data.numberOfAntsSpawned + batchCount)
                    {
                        var spawnedAnts = EntityManager.Instantiate(ant.prefab, batchCount, Allocator.Temp);
                        for (short i = 0; i < batchCount; i++)
                        {
                            var direction = data.random.NextFloat2Direction();
                            var magnitude = data.random.NextFloat();
                            var flatOffset = direction * magnitude * 0.5f;
                            SetComponent(spawnedAnts[i], new Translation {Value = translation.Value + new float3(flatOffset.x, 0, flatOffset.y)});
                            SetComponent(spawnedAnts[i], new ExecutionState {id = (short) (data.numberOfAntsSpawned + i)});
                        }

                        data.numberOfAntsSpawned += batchCount;
                        spawnedAnts.Dispose();
                    }
                }).WithStructuralChanges().Run();
                m_SpawnTimeLeft = GetSingleton<GlobalData>().SpawnInterval;
            }
            else m_SpawnTimeLeft -= Time.DeltaTime;
        }
    }

    public partial class PhysicsLockingSystem : SystemBase
    {
        protected override void OnUpdate() =>
            Entities.WithAll<InertiaNotLocked>().ForEach((Entity e, ref PhysicsMass mass) =>
            {
                mass.InverseInertia = math.up() * 1f;
                EntityManager.RemoveComponent<InertiaNotLocked>(e);
            }).WithStructuralChanges().Run();
    }
}