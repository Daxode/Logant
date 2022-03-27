using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
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
            ref PhysicsVelocity vel, ref AntStack stack,
            in PhysicsMass mass,
            in LocalToWorld ltw, in AntState ant) =>
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
                    var rnd = Random.CreateFromIndex((uint) ant.id);
                    var hasPickedUp = stack.Pop();
                    // Get Target Location
                    float3 targetLocation;
                    if (hasPickedUp)
                    {
                        var targetsFromEntity =  GetBufferFromEntity<DropDownEntityElement>(true);
                        if (targetsFromEntity.HasComponent(executionData.storageEntity))
                        {
                            var targets = targetsFromEntity[executionData.storageEntity];
                            targetLocation = GetComponent<Translation>(targets[rnd.NextInt(targets.Length)]).Value;
                        }
                        else
                        {
                            ecb.DestroyEntity(entityInQueryIndex, e);
                            return;
                        }
                    }
                    else
                    {
                        var targetsFromEntity =  GetBufferFromEntity<PickUpEntityElement>(true);
                        var targets = targetsFromEntity[executionData.storageEntity];
                        targetLocation = GetComponent<Translation>(targets[rnd.NextInt(targets.Length)]).Value;
                    }
                    var flatDir = rnd.NextFloat2Direction() * rnd.NextFloat() * .5f;
                    targetLocation += new float3(flatDir.x,0,flatDir.y);
                    
                    // Calculate Flat Direction to destination
                    var dir = math.normalizesafe(targetLocation - ltw.Position) * .9f;
                    dir *= new float3(1, 0, 1);
                    
                    // Apply Velocity
                    mass.GetImpulseFromForce(dir, ForceMode.Force, in deltaTime, out var impulse, out var massImpulse);
                    vel.ApplyLinearImpulse(in massImpulse, in impulse);
                    vel.ApplyAngularImpulse(in mass, 5f*deltaTime*math.up()*meth.SignedAngle(ltw.Forward, dir, math.up()));
                    break;
            }
        }).ScheduleParallel();
        EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}