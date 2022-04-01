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
        float m_SpawnTimeLeft;
        protected override void OnUpdate()
        {
            if (m_SpawnTimeLeft < 0)
            {
                Entities.ForEach((ref AntHillData data, ref RandomHolder randomHolder, in Translation translation, in AntPrefab ant) =>
                {
                    const ushort batchCount = 5;
                    if (data.Total >= data.Current + batchCount)
                    {
                        var spawnedAnts = EntityManager.Instantiate(ant.prefab, batchCount, Allocator.Temp);
                        for (ushort i = 0; i < batchCount; i++)
                        {
                            var direction = randomHolder.rnd.NextFloat2Direction();
                            var magnitude = randomHolder.rnd.NextFloat();
                            var flatOffset = direction * magnitude * 0.5f;
                            SetComponent(spawnedAnts[i], new Translation {Value = translation.Value + new float3(flatOffset.x, 0, flatOffset.y)});
                            var id = (ushort) (data.Current + i);
                            SetComponent(spawnedAnts[i], new ExecutionState {id = id});
                            SetComponent(spawnedAnts[i], new RandomHolder(Random.CreateFromIndex(id).state));
                        }

                        data.Current += batchCount;
                        spawnedAnts.Dispose();
                    }
                }).WithStructuralChanges().Run();
                m_SpawnTimeLeft = GetSingleton<GlobalData>().SpawnInterval;
            }
            else m_SpawnTimeLeft -= Time.DeltaTime;
        }
    }
}