using System.Collections.Generic;
using Data;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Unity.Transforms;
using Unity.Mathematics;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class AntHillSpawnSystem : SystemBase
    {
        EndFixedStepSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;
        NativeArray<Entity> m_SpawnedEntitiesBuffer;
        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
            m_SpawnedEntitiesBuffer = new NativeArray<Entity>(k_BatchCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
        protected override void OnDestroy() => m_SpawnedEntitiesBuffer.Dispose();

        float m_SpawnTimeLeft;
        const ushort k_BatchCount = 5;
        protected override void OnUpdate()
        {
            if (m_SpawnTimeLeft < 0)
            {
                var spawnedEntitiesBuffer = m_SpawnedEntitiesBuffer;
                var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
                Entities.ForEach((ref AntHillData data, ref RandomHolder randomHolder, in Translation translation, in AntPrefab ant) =>
                {
                    if (data.Total >= data.Current + k_BatchCount)
                    {
                        ecb.Instantiate(ant.prefab, spawnedEntitiesBuffer);
                        for (ushort i = 0; i < k_BatchCount; i++)
                        {
                            var direction = randomHolder.rnd.NextFloat2Direction();
                            var magnitude = randomHolder.rnd.NextFloat();
                            var flatOffset = direction * magnitude * 0.5f;
                            ecb.SetComponent(spawnedEntitiesBuffer[i], new Translation {Value = translation.Value + new float3(flatOffset.x, 0, flatOffset.y)});
                            var id = (ushort) (data.Current + i);
                            ecb.SetComponent(spawnedEntitiesBuffer[i], new ExecutionState {id = id});
                            ecb.SetComponent(spawnedEntitiesBuffer[i], new RandomHolder(Random.CreateFromIndex(id).state));
                        }

                        data.Current += k_BatchCount;
                    }
                }).Run();
                m_SpawnTimeLeft = GetSingleton<GlobalData>().SpawnInterval;
            } else m_SpawnTimeLeft -= Time.DeltaTime;
        }
    }
}