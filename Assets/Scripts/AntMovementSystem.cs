using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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

        Entities.WithReadOnly(colonyExecutionDataBuffer).ForEach((Entity e, int entityInQueryIndex, ref PhysicsVelocity vel, in AntState ant, in Translation translation) =>
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
                    var targetsFromEntity = GetBufferFromEntity<EntityBufferElement>(true);
                    var targets = targetsFromEntity[executionData.storageEntity];
                    var targetLocation = GetComponent<Translation>(targets[ant.id % targets.Length]);
                    vel.Linear = math.normalizesafe(targetLocation.Value - translation.Value) * deltaTime * 100;
                    break;
                case ColonyExecutionType.AntWaitOn:
                    break;
            }
        }).ScheduleParallel();
        EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}