using System.Runtime.CompilerServices;
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
        const ushort k_BatchCount = 3;
        protected override void OnUpdate()
        {
            if (m_SpawnTimeLeft < 0)
            {
                var globalData = GetSingleton<GlobalData>();
                m_SpawnTimeLeft = globalData.SpawnInterval;
                if (!globalData.HasStarted) return;
                
                var spawnedEntitiesBuffer = m_SpawnedEntitiesBuffer;
                var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
                Entities.ForEach((ref AntHillData data, ref RandomHolder randomHolder, in Translation translation, in AntPrefab ant) =>
                {
                    if (data.Total >= data.Current + k_BatchCount)
                    {
                        ecb.Instantiate(ant.prefab, spawnedEntitiesBuffer);
                        for (ushort i = 0; i < k_BatchCount; i++) 
                            SetupAnt(ref ecb, ref randomHolder.rnd, spawnedEntitiesBuffer[i], (ushort) (data.Current + i), in translation.Value);
                        data.Current += k_BatchCount;
                    } else if (data.Total>data.Current) { // If still remaining ants
                        for (ushort i = 0; i < data.Total-data.Current; i++) {
                            var e = ecb.Instantiate(ant.prefab);
                            SetupAnt(ref ecb, ref randomHolder.rnd, e, (ushort)(data.Current+i), in translation.Value);
                        }
                        data.Current = data.Total;
                    }
                }).Run();
            } else m_SpawnTimeLeft -= Time.DeltaTime;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SetupAnt(ref EntityCommandBuffer ecb, ref Random rnd, in Entity entity, in ushort id, in float3 pos)
        {
            var direction = rnd.NextFloat2Direction();
            var magnitude = rnd.NextFloat();
            var flatOffset = direction * magnitude * 0.5f;
            ecb.SetComponent(entity, new Translation {Value = pos + new float3(flatOffset.x, 0, flatOffset.y)});
            ecb.SetComponent(entity, new ExecutionState {id = id});
            ecb.SetComponent(entity, new RandomHolder(Random.CreateFromIndex(id).state));
        }
    }
}