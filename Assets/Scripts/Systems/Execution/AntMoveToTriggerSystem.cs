using System;
using Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Systems.Execution
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class AntMoveToTriggerSystem : SystemBase
    {
        StepPhysicsWorld PhysicsWorld;

        protected override void OnCreate() => PhysicsWorld = World.GetExistingSystem<StepPhysicsWorld>();

        protected override void OnUpdate()
        {
            var antDestJob = new AntMoveToTriggerJob
            {
                antStateFromEntity = GetComponentDataFromEntity<ExecutionState>(),
                foodStoreFromEntity = GetComponentDataFromEntity<FoodStore>(true),
            };

            Dependency = antDestJob.Schedule(PhysicsWorld.Simulation, Dependency);
        }

        struct AntMoveToTriggerJob : ITriggerEventsJob
        {
            // Ant
            public ComponentDataFromEntity<ExecutionState> antStateFromEntity;
            // Location
            [ReadOnly] public ComponentDataFromEntity<FoodStore> foodStoreFromEntity;

            public void Execute(TriggerEvent triggerEvent)
            {
                var entityA = triggerEvent.EntityA;
                var entityB = triggerEvent.EntityB;

                if (antStateFromEntity.HasComponent(entityA) && foodStoreFromEntity.HasComponent(entityB))
                {
                    var antState = antStateFromEntity[entityA];
                    antState.executionLine++;
                    antStateFromEntity[entityA] = antState;
                }
            }
        }
    }
}

