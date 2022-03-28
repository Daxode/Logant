using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class AntExecutionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var colonyExecutionEntity = GetSingletonEntity<ExecutionLine>();
            var colonyExecutionDataBuffer = GetBuffer<ExecutionLine>(colonyExecutionEntity, true);
            
            var deltaTime = Time.DeltaTime;
            Entities.ForEach((ref ExecutionState state, 
                ref PhysicsVelocity vel, in PhysicsMass mass,
                in LocalToWorld ltw) =>
            {
                var executionData = colonyExecutionDataBuffer[state.executionLine];
                var rnd = Random.CreateFromIndex((uint) state.id);

                switch (executionData.type)
                {
                    // Ant Behaviour
                    case ColonyExecutionType.AntMoveTo:
                        // Get Target Location
                        var targetLocation = GetComponent<Translation>(executionData.storageEntity).Value;
                        var flatDir = rnd.NextFloat2Direction() * rnd.NextFloat() * .5f;
                        targetLocation += new float3(flatDir.x, 0, flatDir.y);

                        // Calculate Flat Direction to destination
                        var dir = math.normalizesafe(targetLocation - ltw.Position) * .9f;
                        dir *= new float3(1, 0, 1);

                        // Apply Velocity
                        mass.GetImpulseFromForce(dir, ForceMode.Force, in deltaTime, out var impulse, out var massImpulse);
                        vel.ApplyLinearImpulse(in massImpulse, in impulse);
                        vel.ApplyAngularImpulse(in mass, 5f * deltaTime * math.up() * meth.SignedAngle(ltw.Forward, dir, math.up()));

                        state.executionLine--;
                        break;
                    default: return;
                }

                state.executionLine++;
            }).ScheduleParallel();
        }
    }
}
