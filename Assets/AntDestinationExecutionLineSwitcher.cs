using System.ComponentModel;
using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

public partial class AntDestinationExecutionLineSwitcher : SystemBase
{
    StepPhysicsWorld PhysicsWorld;
    
    protected override void OnCreate()
    {
        PhysicsWorld = World.GetExistingSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var antDestJob = new AntDestinationTriggerJob
        {
            lineFromEntity = GetComponentDataFromEntity<ExecutionLine>(true),
            antStateFromEntity = GetComponentDataFromEntity<AntState>()
        };
        Dependency = antDestJob.Schedule(PhysicsWorld.Simulation, Dependency);
    }
    
    [BurstCompile]
    struct AntDestinationTriggerJob : ITriggerEventsJob
    {
        [Unity.Collections.ReadOnly] public ComponentDataFromEntity<ExecutionLine> lineFromEntity;
        public ComponentDataFromEntity<AntState> antStateFromEntity;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            if (lineFromEntity.HasComponent(entityA) && antStateFromEntity.HasComponent(entityB))
                SetLine(entityB, entityA);
            else if (lineFromEntity.HasComponent(entityA) && antStateFromEntity.HasComponent(entityB))
                SetLine(entityA, entityB);

        }
        
        void SetLine(Entity ant, Entity location)
        {
            var antState = antStateFromEntity[ant];
            antState.executionLine = lineFromEntity[location].line;
            antStateFromEntity[ant] = antState;
        }
    }
}