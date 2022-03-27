using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class AntDestinationExecutionLineSwitcher : SystemBase
{
    StepPhysicsWorld PhysicsWorld;
    
    protected override void OnCreate() => PhysicsWorld = World.GetExistingSystem<StepPhysicsWorld>();

    protected override void OnUpdate()
    {
        var antDestJob = new AntDestinationTriggerJob
        {
            lineFromEntity = GetComponentDataFromEntity<ExecutionLine>(true),
            antStateFromEntity = GetComponentDataFromEntity<AntState>()
        };
        Dependency = antDestJob.Schedule(PhysicsWorld.Simulation, Dependency);
    }

    struct AntDestinationTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<ExecutionLine> lineFromEntity;
        public ComponentDataFromEntity<AntState> antStateFromEntity;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            if (antStateFromEntity.HasComponent(entityA) && lineFromEntity.HasComponent(entityB))
                SetLine(entityA, entityB);

        }
        
        void SetLine(Entity ant, Entity location)
        {
            var antState = antStateFromEntity[ant];
            antState.executionLine++;
            antStateFromEntity[ant] = antState;
        }
    }
}