using System;
using Data;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class ExecutionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;
        protected override void OnCreate() => m_EndSimulationEntityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var colonyExecutionEntity = GetSingletonEntity<ExecutionLine>();
            var colonyExecutionDataBuffer = GetBuffer<ExecutionLine>(colonyExecutionEntity, true);
            var ecb = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities.WithReadOnly(colonyExecutionDataBuffer).ForEach(
                (Entity e, int entityInQueryIndex, ref ExecutionState state, ref Registers registers) =>                         
                {
                    var executionData = colonyExecutionDataBuffer[state.executionLine];

                    switch (executionData.type)
                    {
                        // Control Flow
                        case ColonyExecutionType.GoTo:
                            state.executionLine = (short) (GetComponent<ExecutionLineIndex>(executionData.storageEntity) - 1);
                            break;
                        case ColonyExecutionType.GoToTrue:
                            var registerIndex = GetComponent<RegisterIndex>(executionData.storageEntity);
                            state.executionLine = registers[registerIndex]
                                ? (short) (GetComponent<ExecutionLineIndex>(executionData.storageEntity) - 1)
                                : state.executionLine;
                            break;
                        case ColonyExecutionType.Exit:
                            ecb.RemoveComponent<ExecutionState>(entityInQueryIndex, e);
                            break;

                        // Data
                        case ColonyExecutionType.Set:
                            registers.Enable(GetComponent<RegisterIndex>(executionData.storageEntity));
                            break;
                        case ColonyExecutionType.Copy:
                            var indexes = GetComponent<CopyIndex>(executionData.storageEntity);
                            registers.Write(indexes.To, registers[indexes.From]);
                            break;

                        // Exotic
                        case ColonyExecutionType.GoToRandom:
                            var executionLineFromEntity = GetBufferFromEntity<ExecutionLineIndexElement>(true);
                            var lines = executionLineFromEntity[executionData.storageEntity];
                            var rnd = Random.CreateFromIndex((uint) state.id);
                            state.executionLine = (short) (lines[rnd.NextInt(lines.Length)] - 1);
                            break;
                        default: return;
                    }

                    state.executionLine++;
                }).ScheduleParallel();
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
