using System;
using Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Systems.Execution
{
    [UpdateInGroup(typeof(ExecutionSystemGroup))]
    public partial class AntMoveToTriggerSystem : SystemBase
    {
        StepPhysicsWorld m_PhysicsWorld;

        protected override void OnCreate() => m_PhysicsWorld = World.GetExistingSystem<StepPhysicsWorld>();

        protected override void OnUpdate()
        {
            var executionEntity = GetSingletonEntity<ExecutionLine>();
            var executionLines = GetBuffer<ExecutionLine>(executionEntity, true);
            
            var antDestJob = new AntMoveToTriggerJob
            {
                antStateFromEntity = GetComponentDataFromEntity<ExecutionState>(),
                dataHolderFromEntity = GetComponentDataFromEntity<ExecutionLineDataHolder>(true),
                executionLines = executionLines
            };

            Dependency = antDestJob.Schedule(m_PhysicsWorld.Simulation, Dependency);
        }

        struct AntMoveToTriggerJob : ITriggerEventsJob
        {
            // Ant
            public ComponentDataFromEntity<ExecutionState> antStateFromEntity;
            public DynamicBuffer<ExecutionLine> executionLines;

            // NodeObject
            [ReadOnly] public ComponentDataFromEntity<ExecutionLineDataHolder> dataHolderFromEntity;

            public void Execute(TriggerEvent triggerEvent)
            {
                var entityA = triggerEvent.EntityA;
                var entityB = triggerEvent.EntityB;

                if (antStateFromEntity.HasComponent(entityA) && dataHolderFromEntity.HasComponent(entityB))
                {
                    var antState = antStateFromEntity[entityA];
                    if (executionLines[antState.executionLine].storageEntity == entityB)
                    {
                        antState.executionLine++;
                        antStateFromEntity[entityA] = antState;
                    }
                }
            }
        }
        
        struct AntPickUpTriggerJob : ITriggerEventsJob
        {
            // Ant
            public ComponentDataFromEntity<ExecutionState> antStateFromEntity;
            public DynamicBuffer<ExecutionLine> executionLines;

            // NodeObject
            [ReadOnly] public ComponentDataFromEntity<ExecutionLineDataHolder> dataHolderFromEntity;

            public void Execute(TriggerEvent triggerEvent)
            {
                var entityA = triggerEvent.EntityA;
                var entityB = triggerEvent.EntityB;

                if (antStateFromEntity.HasComponent(entityA) && dataHolderFromEntity.HasComponent(entityB))
                {
                    var antState = antStateFromEntity[entityA];
                    if (executionLines[antState.executionLine].storageEntity == entityB)
                    {
                        antState.executionLine++;
                        antStateFromEntity[entityA] = antState;
                    }
                }
            }
        }
    }
}

