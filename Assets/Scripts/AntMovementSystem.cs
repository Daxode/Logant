using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

public partial class AntMovementSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem EndSimulationEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        EndSimulationEntityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var colonyExecutionEntity = GetSingletonEntity<ColonyExecutionData>();
        var colonyExecutionDataBuffer = GetBuffer<ColonyExecutionData>(colonyExecutionEntity, true);
        var deltaTime = Time.DeltaTime;
        var ecb = EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Entities.WithReadOnly(colonyExecutionDataBuffer).ForEach((Entity e, int entityInQueryIndex, 
            ref PhysicsVelocity vel, in PhysicsMass mass,
            in AntState ant, in LocalToWorld ltw) =>
        {
            var executionData = colonyExecutionDataBuffer[ant.executionLine];

            switch (executionData.type)
            {
                case ColonyExecutionType.GoTo:
                    break;
                case ColonyExecutionType.Stop:
                    ecb.RemoveComponent<AntState>(entityInQueryIndex, e);
                    break;
                case ColonyExecutionType.AntMoveTo:
                    // Get Target Location
                    var targetsFromEntity = GetBufferFromEntity<EntityBufferElement>(true);
                    var targets = targetsFromEntity[executionData.storageEntity];
                    var targetLocation = GetComponent<Translation>(targets[(int) (ant.id % targets.Length)]).Value;
                    var rnd = Random.CreateFromIndex(ant.id);
                    var flatDir = rnd.NextFloat2Direction() * rnd.NextFloat() * .5f;
                    targetLocation += new float3(flatDir.x,0,flatDir.y);
                    
                    // Calculate Flat Direction to destination
                    var dir = math.normalizesafe(targetLocation - ltw.Position) * 15;
                    dir *= new float3(1, 0, 1);
                    
                    // Apply Velocity
                    mass.GetImpulseFromForce(dir, ForceMode.Force, in deltaTime, out var impulse, out var massImpulse);
                    vel.ApplyLinearImpulse(in massImpulse, in impulse);
                    vel.ApplyAngularImpulse(in mass, deltaTime*math.up()*meth.SignedAngle(ltw.Forward, dir, math.up()));
                    break;
                case ColonyExecutionType.AntWaitOn:
                    break;
            }
        }).ScheduleParallel();
        EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}