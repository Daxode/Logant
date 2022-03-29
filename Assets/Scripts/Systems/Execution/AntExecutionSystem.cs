using System;
using Data;
using Systems.Execution;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [UpdateInGroup(typeof(ExecutionSystemGroup))]
    public partial class AntExecutionSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;
        protected override void OnCreate() => m_EndSimulationEntityCommandBufferSystem=World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var executionEntity = GetSingletonEntity<ExecutionLine>();
            var executionLines = GetBuffer<ExecutionLine>(executionEntity, true);
            var ecb = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            
            var deltaTime = Time.DeltaTime;
            Entities.WithReadOnly(executionLines).ForEach((Entity e, int entityInQueryIndex,
                ref ExecutionState state, 
                ref PhysicsVelocity vel, in PhysicsMass mass,
                in LocalToWorld ltw) =>
            {
                var executionData = executionLines[state.executionLine];

                switch (executionData.type)
                {
                    // Ant Behaviour
                    case ExecutionType.AntMoveTo:
                        var rnd = Random.CreateFromIndex(state.id);
                        
                        // Get Target NodeObject
                        var targetLocation = GetComponent<Translation>(executionData.ePtr).Value;
                        var flatDir = rnd.NextFloat2Direction() * rnd.NextFloat() * .5f;
                        targetLocation += new float3(flatDir.x, 0, flatDir.y);

                        // Calculate Flat Direction to destination
                        var dir = math.normalizesafe(targetLocation - ltw.Position) * .9f;
                        dir *= new float3(1, 0, 1);

                        // Apply Velocity
                        mass.GetImpulseFromForce(dir, ForceMode.Force, in deltaTime, out var impulse, out var massImpulse);
                        vel.ApplyLinearImpulse(in massImpulse, in impulse);
                        vel.ApplyAngularImpulse(in mass, 5f * deltaTime * math.up() * meth.SignedAngle(ltw.Forward, dir, math.up()));

                        state.executionLine--; // Keeps running the above code until external source increments.
                        break;
                    case ExecutionType.AntDestroy:
                        ecb.DestroyEntity(entityInQueryIndex, e);
                        return;
                    default: return;
                }

                state.executionLine++;
            }).ScheduleParallel();
            
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
