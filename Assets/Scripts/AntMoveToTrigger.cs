using Data;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class AntMoveToTrigger : SystemBase
{
    StepPhysicsWorld PhysicsWorld;
    
    protected override void OnCreate() => PhysicsWorld = World.GetExistingSystem<StepPhysicsWorld>();

    protected override void OnUpdate()
    {
        var antDestJob = new AntDestinationTriggerJob
        {
            indexFromEntity = GetComponentDataFromEntity<RegisterIndex>(true),
            antStateFromEntity = GetComponentDataFromEntity<ExecutionState>()
        };
        Dependency = antDestJob.Schedule(PhysicsWorld.Simulation, Dependency);
    }

    struct AntDestinationTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<RegisterIndex> indexFromEntity;
        public ComponentDataFromEntity<ExecutionState> antStateFromEntity;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            if (antStateFromEntity.HasComponent(entityA) && indexFromEntity.HasComponent(entityB))
            {
                var antState = antStateFromEntity[entityA];
                antState.executionLine++;
                antStateFromEntity[entityA] = antState;
            }

        }
    }
}