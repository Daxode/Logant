using System;
using Data;
using Systems.Execution;
using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [UpdateInGroup(typeof(ExecutionSystemGroup))]
    public partial class ExecutionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;
        protected override void OnCreate() => m_EndSimulationEntityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var executionEntity = GetSingletonEntity<ExecutionLine>();
            var executionLines = GetBuffer<ExecutionLine>(executionEntity, true);
            var ecb = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities.WithReadOnly(executionLines).ForEach(
                (Entity e, int entityInQueryIndex, ref ExecutionState state, ref Registers registers, ref RandomHolder rndHolder) =>                         
                {
                    var line = executionLines[state.executionLine];

                    switch (line.type)
                    {
                        // Control Flow
                        case ExecutionType.GoTo:
                            state.executionLine = (ushort) (GetComponent<ExecutionLineIndex>(line.ePtr) - 1);
                            break;
                        case ExecutionType.GoToTrue:
                            var registerIndex = GetComponent<RegisterIndex>(line.ePtr);
                            state.executionLine = registers[registerIndex]
                                ? (ushort) (GetComponent<ExecutionLineIndex>(line.ePtr) - 1)
                                : state.executionLine;
                            break;
                        case ExecutionType.Exit:
                            ecb.RemoveComponent<ExecutionState>(entityInQueryIndex, e);
                            break;

                        // Data
                        case ExecutionType.Set:
                            registers.Set(GetComponent<RegisterIndex>(line.ePtr));
                            break;
                        case ExecutionType.Copy:
                            var copyIndexes = GetComponent<CopyIndex>(line.ePtr);
                            registers.Write(copyIndexes.To, registers[copyIndexes.From]);
                            break;
                        case ExecutionType.CmpLT:
                            var cmpLTIndexes = GetBufferFromEntity<RegisterIndexElement>(true)[line.ePtr];
                            byte registerA = cmpLTIndexes[0];
                            byte registerB = cmpLTIndexes[1];
                            byte count = GetComponent<Count>(line.ePtr);
                            var valA = registers.Read(registerA, count);
                            var valB = registers.Read(registerB, count);
                            byte registerSave = cmpLTIndexes[2];
                            registers.Write(registerSave, valA<valB);
                            break;

                        // Exotic
                        case ExecutionType.GoToRandom:
                            var executionLineFromEntity = GetBufferFromEntity<ExecutionLineIndexElement>(true);
                            var lines = executionLineFromEntity[line.ePtr];
                            state.executionLine = (ushort) (lines[rndHolder.rnd.NextInt(lines.Length)] - 1);
                            break;
                        case ExecutionType.SkipAFrame:
                            break;
                        default: return;
                    }

                    state.executionLine++;
                }).ScheduleParallel();
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}

public struct TargetEntity : IComponentData
{ 
    Entity m_E;
    TargetEntity(Entity e) => m_E = e;
    public static implicit operator TargetEntity(Entity e) => new TargetEntity(e);
    public static implicit operator Entity(TargetEntity t) => t.m_E;
}